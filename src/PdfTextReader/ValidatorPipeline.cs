﻿using System;
using System.Collections.Generic;
using System.Text;
using PdfTextReader.Base;
using PdfTextReader.Parser;
using PdfTextReader.ExecutionStats;
using PdfTextReader.TextStructures;
using PdfTextReader.Execution;
using PdfTextReader.PDFText;
using System.Drawing;
using PdfTextReader.PDFCore;
using PdfTextReader.PDFValidation;

namespace PdfTextReader
{
    public class ValidatorPipeline
    {
        public static void Process(string basename, string inputfolder, string outputfolder)
        {
            //PdfReaderException.DisableWarnings();
            //PdfReaderException.ContinueOnException();
            
            var pipeline = new Execution.Pipeline();

            var result =
            pipeline.Input($"{inputfolder}/{basename}")
                    .Output($"{outputfolder}/{basename}-invalid.pdf")
                    .AllPagesExcept<CreateTextLines>(new int[] { }, page =>
                              page.ParsePdf<ProcessPdfValidation>()
                                  .Show(Color.White)
                                  .ParseBlock<IdentifyValidationMarks>()
                                  .LogCheck<MarkOrangeNoOverlap>(Color.Orange)   
                                  .Show(Color.Blue)
                    ).ToList();

            pipeline.SaveOk($"{outputfolder}/{basename}-ok.pdf");
            pipeline.SaveErrors($"{outputfolder}/{basename}-errors.pdf");

            pipeline.Done();
        }
        public static void ProcessPage1(string basename, string inputfolder, string outputfolder)
        {
            //PdfReaderException.DisableWarnings();
            //PdfReaderException.ContinueOnException();

            var pipeline = new Execution.Pipeline();

            var result =
            pipeline.Input($"{inputfolder}/{basename}")
                    .Output($"{outputfolder}/{basename}-invalid.pdf")
                    .Page(1)
                                .ParsePdf<ProcessPdfValidation>()
                                  .Show(Color.White)
                                  .ParseBlock<IdentifyValidationMarks>()
                                  .ParseBlock<MarkOrangeNoOverlap>()
                                  .Show(Color.Blue);
                    
            pipeline.Done();
       }
    }
}
