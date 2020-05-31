using System;
using System.Collections.Generic;

namespace IT.Scrapper.Domain.Contracts
{
    public class DownloadedPost
    {
        public string Title { get; set; }
        public IEnumerable<string> PostContents { get; set; }
    }
}