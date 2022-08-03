using System;
using System.Collections.Generic;
using System.Linq;

namespace CsCodeGenerator
{
    public class FileModel
    {
        public FileModel() { }
        public FileModel(string name)
        {
            Name = name;
        }

        public List<string> UsingDirectives { get; set; } = new List<string>();

        public string Namespace { get; set; }

        public string Name { get; set; }

        public string Header { get; set; }

        public string Extension { get; set; } = Util.CsExtension;

        public string FullName => Name + "." + Extension;

        public List<EnumModel> Enums { get; set; } = new List<EnumModel>();

        public List<ClassModel> Classes { get; set; } = new List<ClassModel>();
        
        public List<StructModel> Structs { get; set; } = new List<StructModel>();
        
        public List<InterfaceModel> Interfaces { get; set; } = new List<InterfaceModel>();

        public void LoadUsingDirectives(List<string> usingDirectives)
        {
            foreach (var usingDirective in usingDirectives)
            {
                UsingDirectives.Add(usingDirective);
            }
        }

        public override string ToString()
        {
            string usingText = UsingDirectives.Count > 0 ? Util.Using + " " : "";
            var headerText = !string.IsNullOrWhiteSpace(Header) ? Header + Util.NewLine : "";
            string result = headerText + usingText + String.Join(Util.NewLine + usingText, UsingDirectives);
            //result += string.IsNullOrEmpty(Namespace) ? "" : Util.NewLineDouble + Util.Namespace + " " + Namespace;
            //result += Util.NewLine + "{";
            if (string.IsNullOrEmpty(Namespace))
            {
                Enums.ForEach(ReduceIndent);
                Classes.ForEach(ReduceIndent);
                Structs.ForEach(ReduceIndent);
                Interfaces.ForEach(ReduceIndent);
            }

            result += Util.NewLine;
            result += string.Join(Util.NewLine, GetSourceElements());
            //result += Util.NewLine + "}";
            result += Util.NewLine;
            return result;
        }

        private IEnumerable<string> GetSourceElements()
        {
            foreach (var o in Enums)
            {
                if (o != null)
                {
                    yield return o.ToString();
                }
            }
            foreach (var o in Classes)
            {
                if (o != null)
                {
                    yield return o.ToString();
                }
            }
            foreach (var o in Structs)
            {
                if (o != null)
                {
                    yield return o.ToString();
                }
            }
            foreach (var o in Interfaces)
            {
                if (o != null)
                {
                    yield return o.ToString();
                }
            }
        }

        private static void ReduceIndent(BaseElement element)
        {
            if (element != null)
            {
                element.IndentSize -= CsGenerator.DefaultTabSize;
            }
        }

        private static void ReduceIndent(InterfaceModel element)
        {
            ReduceIndent((BaseElement)element);
            element?.Methods.ForEach(ReduceIndent);
        }

        private static void ReduceIndent(ClassModel element)
        {
            ReduceIndent((BaseElement)element);
            element?.Methods.ForEach(ReduceIndent);
            element?.Properties.ForEach(ReduceIndent);
            element?.Fields.ForEach(ReduceIndent);
        }

        private static void ReduceIndent(StructModel element)
        {
            ReduceIndent((BaseElement)element);
            element?.Methods.ForEach(ReduceIndent);
            element?.Properties.ForEach(ReduceIndent);
            element?.Fields.ForEach(ReduceIndent);
        }
    }
}
