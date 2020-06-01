using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace breyn
{

    [Verb("add", HelpText = "Add something to breyn.")]
    public class AddOptions {

        [Option(Required = true, HelpText = "Value")]
        public string Value { get; set; }

        [Option(Required = false, HelpText = "Tag")]
        public string Tag { get; set; }

        [Option(Required = false, HelpText = "Reminder")]
        public string Reminder { get; set; }


        [Option('t', Separator=':')]
        public IEnumerable<string> Types { get; set; }
    }

    [Verb("get", HelpText = "Get something from breyn.")]
    public class GetOptions {

        [Option(Required = true, HelpText = "Value")]
        public string Value { get; set; }

        [Option(Required = false, HelpText = "Tag")]
        public string Tag { get; set; }

    }

    [Verb("remove", HelpText = "Remove something from breyn.")]
    public class RemoveOptions {
        [Option(Required = true, HelpText = "Value")]
        public string Value { get; set; }

        [Option(Required = true, HelpText = "Tag")] 
        public string Tag { get; set; }

    }

  


}
