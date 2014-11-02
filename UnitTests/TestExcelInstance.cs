﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using XLToolbox.Excel.Instance;
using Microsoft.Office.Interop.Excel;

namespace XLToolbox.Test
{
    [TestFixture]
    class TestExcelInstance
    {
        [Test]
        public void StartAndQuitExcel()
        {
            ExcelInstance.Start();
            ExcelInstance.Shutdown();
        }

        [Test]
        public void QuittingExcelWhileInstanceExistsThrows()
        {
            ExcelInstance.Start();
            ExcelInstance.Application.Visible = true;
            Workbook wb = ExcelInstance.Application.Workbooks[1] as Workbook;
            Worksheet sh = wb.Worksheets[1] as Worksheet;
            sh.Cells[1, 1] = "Hello World";
            using (ExcelInstance i = new ExcelInstance())
            {
                Assert.Throws(typeof(ExcelInstanceException), () =>
                {
                    ExcelInstance.Shutdown();
                });
            }
            ExcelInstance.Shutdown();
        }

        [Test]
        public void CreateWorkbook()
        {
            ExcelInstance.Start();
            Workbook wb = ExcelInstance.CreateWorkbook();
            Assert.AreEqual(1, wb.Sheets.Count,
                String.Format("Created workbook should have 1 worksheet but has {0}", 
                 wb.Sheets.Count
                )
            );
            int n = 5;
            wb = ExcelInstance.CreateWorkbook(n);
            Assert.AreEqual(n, wb.Sheets.Count,
                String.Format("Created workbook should have {0} worksheets but has {1}",
                 n, wb.Sheets.Count
                )
            );
            ExcelInstance.Shutdown();
        }
    }
}
