﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitterLib.Load.Mergers
{
    class TweetUrl : Merger
    {
        protected override string SourceTableName
        {
            get { return "tweet_url"; }
        }

        protected override string TargetTableName
        {
            get { return "tweet_url"; }
        }
    }
}
