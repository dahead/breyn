using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;

namespace breyn
{
    public class breyn
    {

        public Index idx = new Index();

        public void Init(string[] args)
        {

            idx.Load();

             // Parse arguments
            CommandLine.Parser.Default.ParseArguments<AddOptions, GetOptions, RemoveOptions>(args)
                .MapResult(
                (AddOptions opts) => RunAddAndReturnExitCode(opts),
                (GetOptions opts) => RunGetAndReturnExitCode(opts),
                (RemoveOptions opts) => RunRemoveAndReturnExitCode(opts),
                errs => HandleParseError(errs));
        }

        private int HandleParseError(IEnumerable<CommandLine.Error> errors)
        {
            return 0;
        }

        private int RunAddAndReturnExitCode(AddOptions opt)
		{	

            idx.Items.Add(new Item() { Value = opt.Value, Tag = opt.Tag } );
            
            idx.Save();

            return 0;
		}

        private int RunGetAndReturnExitCode(GetOptions opt)
		{	
            return 0;
		}
		
        private int RunRemoveAndReturnExitCode(RemoveOptions opt)
		{	
            return 0;
		}

    }
}