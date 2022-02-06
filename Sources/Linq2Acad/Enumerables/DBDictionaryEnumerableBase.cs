﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace Linq2Acad
{
  public abstract class DBDictionaryEnumerable<T> : NameBasedContainerEnumerableBase<T> where T : DBObject
  {
    protected DBDictionaryEnumerable(Database database, Transaction transaction, ObjectId containerID)
      : base(database, transaction, containerID, i => (ObjectId)((DictionaryEntry)i).Value)
    {
    }

    protected T AddInternal(T newItem, string name)
    {
      AddRangeInternal(new[] { (newItem, name) });
      return newItem;
    }

    protected void AddRangeInternal(IEnumerable<(T Item, string Name)> items)
    {
      var dict = (DBDictionary)transaction.GetObject(ID, OpenMode.ForWrite);

      foreach (var item in items)
      {
        dict.SetAt(item.Name, item.Item);
        transaction.AddNewlyCreatedDBObject(item.Item, true);
      }
    }

    protected override sealed bool ContainsInternal(ObjectId id)
      => ((DBDictionary)transaction.GetObject(ID, OpenMode.ForRead)).Contains(id);

    protected override sealed bool ContainsInternal(string name)
      => ((DBDictionary)transaction.GetObject(ID, OpenMode.ForRead)).Contains(name);

    public T Element(string name, bool openForWrite = false)
    {
      Require.StringNotEmpty(name, nameof(name));
      Require.NameExists<T>(Contains(name), name);

      return ElementInternal(name, openForWrite);
    }

    private T ElementInternal(string name, bool openForWrite)
    {
      var dict = (DBDictionary)transaction.GetObject(ID, OpenMode.ForRead);
      var id = dict.GetAt(name);
      return (T)transaction.GetObject(id, openForWrite ? OpenMode.ForWrite : OpenMode.ForRead);
    }

    public T ElementOrDefault(string name, bool openForWrite = false)
    {
      Require.StringNotEmpty(name, nameof(name));

      return ElementOrDefaultInternal(name, openForWrite);
    }

    private T ElementOrDefaultInternal(string name, bool openForWrite)
    {
      var dict = (DBDictionary)transaction.GetObject(ID, openForWrite ? OpenMode.ForWrite : OpenMode.ForRead);

      if (dict.Contains(name))
      {
        var id = dict.GetAt(name);
        return (T)transaction.GetObject(id, OpenMode.ForRead);
      }
      else
      {
        return null;
      }
    }

    public override sealed int Count()
      => ((DBDictionary)transaction.GetObject(ID, OpenMode.ForRead)).Count;
  }
}