﻿using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace XLToolbox.WorkbookStorage
{
    public class Store : Object
    {
        private Dictionary<string, ContextItems> _contexts;
        private const string STORESHEETNAME = "_xltb_storage_";
        private const string STORESHEETINFO = "XL Toolbox Settings";
        private const int FIRSTROW = 2;
        private string _context;
        protected bool Dirty { get; set; }

        private Workbook _workbook;

        /// <summary>
        /// Gets or sets the associated workbook. If a new workbook is set,
        /// writes the values to the old workbook, then reads the values from
        /// the new workbook.
        /// </summary>
        public Workbook Workbook
        {
            get
            {
                return _workbook;
            }
            set
            {
                if (Dirty)
                {
                    WriteToWorksheet();
                }
                _workbook = value;
                if (_workbook != null)
                {
                    ReadFromWorksheet();
                }
                else
                {
                    _contexts.Clear();
                }
            }
        }

        private Worksheet _storeSheet;
        protected Worksheet StoreSheet {
            get {
                if (_storeSheet == null) {
                    if (Workbook == null)
                    {
                        throw new WorkbookStorageException("Cannot access storage worksheet: no workbook is associated");
                    }
                    try
                    {
                        _storeSheet = Workbook.Worksheets[STORESHEETNAME];
                    }
                    catch (System.Runtime.InteropServices.COMException)
                    {
                        // If the COMException is raised, the worksheet likely does not exist
                        _storeSheet = Workbook.Worksheets.Add();

                        // xlSheetVeryHidden hides the sheet so much that it cannot be made
                        // visible from the Excel graphical user interface
                        _storeSheet.Visible = XlSheetVisibility.xlSheetVeryHidden;

                        // Give the worksheet a special name
                        _storeSheet.Name = STORESHEETNAME;
                    }
                }
                return _storeSheet;
            }
        }

        /// <summary>
        /// Sets the context of the current storage object. This may be a
        /// worksheet name or an empty string for the global context of
        /// the workbook.
        /// </summary>
        // TODO: Deal with worksheets that are renamed or deleted while the WorkbookStorage is instantiated.
        public string Context {
            get
            {
                return _context;
            }
            set
            {
                /// If context is not an empty string, it denotes a worksheet
                /// in the associated workbook. The setter will test if the workbook
                /// does indeed contain such a worksheet, throwing an exception if not.
                if (value.Length > 0) {
                    try {
                        object o = Workbook.Sheets[value];
                        _context = value;
                    }
                    catch (System.Runtime.InteropServices.COMException e) {
                        throw new InvalidContextException(
                            String.Format("Workbook has no sheet named {0}", _context), e);
                    }
                }
                else
                {
                    _context = value;
                }
            }
        }

        internal ContextItems Items
        {
            get
            {
                if (_context == null)
                {
                    throw new UndefinedContextException();
                }
                if (_contexts.ContainsKey(_context))
                {
                    return _contexts[_context];
                }
                else
                {
                    ContextItems c = new ContextItems();
                    _contexts.Add(_context, c);
                    return c;
                }
            }
        }

        protected Store()
        {
            _context = "";
            _contexts = new Dictionary<string, ContextItems>();
        }

        /// <summary>
        /// Instantiates the class and associates it with the active workbook of the
        /// given application.
        /// </summary>
        /// <param name="application">Instance of an Excel application.</param>
        public Store(Application application) : this()
        {
            Workbook = application.ActiveWorkbook;
            UseActiveSheet();
        }

        /// <summary>
        /// Instantiates the class and associates it with a workbook.
        /// </summary>
        /// <param name="workbook">Workbook object to associate the storage with.</param>
        public Store(Workbook workbook) : this()
        {
            this.Workbook = workbook;
            UseActiveSheet();
        }

        ~Store()
        {
            if (Dirty)
            {
                WriteToWorksheet();
            }
        }

        /// <summary>
        /// Retrieves an integer from the storage, given a key. Throws a
        /// WorkbookStorageException if the key is not found.
        /// </summary>
        /// <param name="key">Key to look up.</param>
        /// <returns>Integer value</returns>
        public int Get(string key)
        {
            dynamic v = GetDynamicValue(key);
            if (v == null) {
                throw new UnkownKeyException(String.Format("Context {0} has no key {1}", _context, key));
            }
            return (int)v;
        }

        public void Put(string key, int i)
        {
            PutObject(key, i);
        }

        public void Put(string key, string s)
        {
            PutObject(key, s);
        }

        public void Put(string key, bool b)
        {
            PutObject(key, b);
        }

        /// <summary>
        /// Central method to put objects into the store.
        /// </summary>
        /// <param name="key">Key to store the object under.</param>
        /// <param name="o">Object to store.</param>
        protected void PutObject(string key, object o)
        {
            Item item = new Item(key, Context, o);
            Items.Add(item.key, item);
            Dirty = true;
        }

        /// <summary>
        /// Sets the active sheet of the current workbook as the context.
        /// </summary>
        public void UseActiveSheet()
        {
            Context = Workbook.ActiveSheet.Name;
        }

        /// <summary>
        /// Determines if the current context has a stored item with given key.
        /// </summary>
        /// <param name="key">Key to look up.</param>
        /// <returns>True if key exists in current context, false if not.</returns>
        public bool HasKey(string key)
        {
            return Items.ContainsKey(key);
        }

        /// <summary>
        /// Writes out the values to the hidden worksheet, if values were changed.
        /// </summary>
        public void Flush()
        {
            if (Dirty)
            {
                WriteToWorksheet();
            };
        }

        protected dynamic GetDynamicValue(string key)
        {
            ContextItems c = Items;
            if (c.ContainsKey(key))
            {
                return c[key].value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads all settings from the hidden storage worksheet
        /// </summary>
        protected void ReadFromWorksheet()
        {
            _contexts.Clear();
            Range r = StoreSheet.UsedRange;

            // The first row on a storage worksheet is reserved for internal
            // use (e.g., flags).
            Item item;
            ContextItems context;
            for (int row = FIRSTROW; row <= r.Rows.Count; row++)
            {
                item = new Item(_storeSheet, row);
                if (_contexts.ContainsKey(item.context))
                {
                    context = _contexts[item.context];
                }
                else
                {
                    context = new ContextItems();
                    _contexts.Add(item.context, context);
                };
                context.Add(item.key, item);
            }
        }

        /// <summary>
        /// Writes all settings to the hidden storage worksheet.
        /// </summary>
        protected void WriteToWorksheet()
        {
            // Delete the used range first
            PrepareStoreSheet();

            // Output everything to the sheet.
            int row = FIRSTROW;
            foreach (ContextItems context in _contexts.Values)
            {
                foreach (Item item in context.Values)
                {
                    item.WriteToSheet(_storeSheet, row);
                    row++;
                }
            };
            Dirty = false;
        }

        private void PrepareStoreSheet()
        {
            _storeSheet.UsedRange.Clear();

            // Put an informative string into the first cell;
            // this is also required in order for GetUsedRange() to return
            // the correct range.
            _storeSheet.Cells[1, 1] = STORESHEETINFO;
        }
    }
}
