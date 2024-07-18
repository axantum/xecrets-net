using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Api.Model
{
    public class ListFilterOptions
    {
        public string Id { get; set; }

        public string SearchQuery { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool AllItem { get; set; }
    }
}