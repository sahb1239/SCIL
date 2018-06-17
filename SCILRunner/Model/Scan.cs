using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SCILRunner.Model
{
    public class Scan
    {
        [Key]
        public long Id { get; set; }

        public string FilePath { get; set; }
        public string OutputPath { get; set; }

        public bool Finished { get; set; }

        public ScanStatus Status { get; set; }
    }
}
