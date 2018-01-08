﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfTextReader.PDFCore
{
    class RemoveTableText : IProcessBlock
    {
        private List<IBlock> _tables;

        public RemoveTableText()
        {
            var parserTable = Execution.PipelineFactory.Create<PDFCore.IdentifyTables>();
            var page = parserTable.PageTables;

            if (page == null)
                throw new InvalidOperationException("RemoveTableText requires IdentifyTables");

            this._tables = page.AllBlocks.ToList();
        }

        public BlockPage Process(BlockPage page)
        {
            var result = new BlockPage();

            foreach(var block in page.AllBlocks)
            {
                bool insideTable = false;

                foreach(var table in _tables)
                {
                    if( Block.HasOverlap(table, block) )
                    {
                        insideTable = true;
                        break;
                    }
                }

                if( !insideTable )
                {
                    result.Add(block);
                }
            }

            return result;
        }
    }
}