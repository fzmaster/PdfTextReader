﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserFrontend.Logic
{
    public class PrettyTextFile
    {
        const int TABSTOP_WIDTH = 8;

        int _width;

        public PrettyTextFile()
        {
            _width = 80;
        }

        public void SetWidth(int width)
        {
            _width = width;
        }

        public void SetWidth(string text)
        {
            int width = text.Split('\n').Max(l => l.Length);

            if (width < 60)
                return;

            _width = width;
        }

        public string ProcessCustomizedSections(string text)
        {
            string[] sections = text.Replace("\r", "").Split("\n\n\n");
            StringBuilder stringBuilder = new StringBuilder();

            bool isFirstPage = true;

            foreach(var sec in sections)
            {
                SetWidth(sec);

                if(isFirstPage)
                {
                    isFirstPage = false;

                    if (sec.Contains("Sumário") && sec.Contains("..."))
                    {
                        var sumarioSection = ProcessSumario(sec);

                        stringBuilder.AppendLine(sumarioSection);
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine();

                        continue;
                    }
                }

                var processedSection = Process(sec);

                stringBuilder.AppendLine(processedSection);
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
            }

            string result = stringBuilder.ToString();

            return result;
        }

        public string ProcessSumario(string text)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string[] linhas = text.Replace("\r", "").Split("\n", StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in linhas)
            {
                string[] componentes = line.Split(".", 2);

                if(componentes.Length >= 2)
                {
                    int length = _width - componentes[0].Length - componentes[1].Length;
                    string justifiedLine = componentes[0] + "".PadLeft(length, '.') + componentes[1];

                    stringBuilder.AppendLine(justifiedLine);
                }
                else
                {
                    bool isRight = line.StartsWith("\t\t\t\t");
                    bool isCentered = line.StartsWith("\t\t") && (!isRight);

                    if(isCentered)
                    {
                        stringBuilder.AppendLine(line.TrimStart('\t').PadLeft((_width-line.Length)/2));
                    }
                    else
                    {
                        stringBuilder.AppendLine(line.TrimStart('\t').PadLeft(_width));
                    }                    
                }
            }
            
            var result = stringBuilder.ToString();

            return result;
        }

        public string Process(string text)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string[] paragraphs = text.Replace("\r", "").Split("\n\n", StringSplitOptions.RemoveEmptyEntries);

            var processedParagraphs = paragraphs.Select(ProcessParagraphs).ToArray();

            foreach(var p in processedParagraphs)
            {
                foreach(var line in p)
                {
                    stringBuilder.AppendLine(line);
                }
                stringBuilder.AppendLine();
            }

            var result = stringBuilder.ToString();

            return result;
        }

        IEnumerable<string> ProcessParagraphs(string paras)
        {
            if (paras == "\n")
                return new string[] { };

            bool hasImageOrTable = paras.Contains("[[[IMG(") || paras.Contains("[[[TABLE");

            if (hasImageOrTable)
                return ProcessParagraphsCenter(paras);

            bool isAlignRight = paras.StartsWith("\t\t\t\t");
            bool isAlignCenter = paras.StartsWith("\t\t") && (!isAlignRight);
            
            if(isAlignRight)
            {
                return ProcessParagraphsRight(paras);
            } 
            else if(isAlignCenter)
            {
                return ProcessParagraphsCenter(paras);
            }

            return ProcessParagraphsJustified(paras);
        }

        IEnumerable<string> ProcessParagraphsRight(string paras)
        {
            return ProcessLines(paras, line => {
                line = line.Replace("    ", " ").Replace("  ", "");

                // position at 60% of width, so we fill 40%
                string floatLine = line.PadRight((int)(.40F * _width));

                return floatLine.PadLeft(_width);
            });
        }

        IEnumerable<string> ProcessParagraphsCenter(string paras)
        {
            return ProcessLines(paras, line => {
                int lineWidth = line.Length;
                int prefixCount = (_width - lineWidth)/2;
                
                return line.PadLeft(prefixCount + lineWidth);
            });
        }

        IEnumerable<string> ProcessParagraphsJustified(string paras)
        {
            string[] lines = paras.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            int lastLine = lines.Length - 1;

            for (int i=0; i<lastLine; i++)
            {
                string line = lines[i];

                bool hasTabStop = line.StartsWith("\t");

                string justifiedLine;

                if(hasTabStop)
                {
                    string lineTabStop = line.Trim('\t');
                    int lineInitialTabStop = _width - TABSTOP_WIDTH;

                    string lineInitial = ProcessLineJustified(lineTabStop, lineInitialTabStop);
                    justifiedLine = lineInitial.PadLeft(lineInitial.Length + TABSTOP_WIDTH);
                }
                else
                {
                    justifiedLine = ProcessLineJustified(line, _width);
                }

                yield return justifiedLine;
            }

            string keepSameLine = lines[lastLine].Trim();
            yield return keepSameLine;
        }

        IEnumerable<string> ProcessLines(string paras, Func<string,string> applyTransformation)
        {
            string[] lines = paras.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            return lines.Select(CleanSpaces).Select(applyTransformation);
        }

        string CleanSpaces(string line) => line.Trim('\t', '\r', ' ');

        static string ProcessLineJustified(string line, int line_width)
        {
            string[] words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int wordCount = words.Length;
            int wordLength = words.Sum(w => w.Length);
            int spacesCount = words.Length - 1;
            int requiredLength = line_width - wordLength;

            if (spacesCount == 0 || requiredLength <= 0)
                return line.Trim();
            
            int avgRequiredSpaces = (int)requiredLength / (int)spacesCount;

            // too much?
            if (avgRequiredSpaces > 2)
                avgRequiredSpaces = 2;

            if (avgRequiredSpaces == 0 )
                return line.Trim();

            int additionalSpaces = requiredLength - (avgRequiredSpaces * spacesCount);
            int initialAvgSpaces = spacesCount - additionalSpaces;

            var justifiedLine = new StringBuilder();

            string avgSpace = "".PadRight(avgRequiredSpaces);

            for (int i=0; i< wordCount; i++)
            {
                bool isLastLine = (i == wordCount - 1);

                justifiedLine.Append(words[i]);

                // dont add space at the last line
                if (isLastLine)
                    break;

                justifiedLine.Append(avgSpace);

                if (i>=initialAvgSpaces)
                {
                    justifiedLine.Append(" ");
                }
            }

            string result = justifiedLine.ToString();

            return result;
        }
    }
}
