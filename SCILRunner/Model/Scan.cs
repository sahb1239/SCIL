using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public virtual ICollection<DataPoint> Datapoint { get; set; } = new List<DataPoint>();

    }

    public class DataPoint
    {
        [Key]
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public long MemoryUsage { get; set; }
        public long FlixProcesses { get; set; }

    }
}
