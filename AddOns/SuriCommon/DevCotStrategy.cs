﻿using System.Collections.Generic;
using System.Linq;

namespace NinjaTrader.Custom.AddOns.SuriCommon {
    public static class SuriCotStrategy {
        static SuriCotStrategy() {
            releaseToReportDates = reportToReleaseDates.ToDictionary(pair => pair.Value, pair => pair.Key);
        }
        public static readonly Dictionary<string, string> releaseToReportDates;
        public static readonly Dictionary<string, string> reportToReleaseDates = new Dictionary<string, string> {
            {"2022-03-29", "2022-04-01"},
            {"2022-03-22", "2022-03-25"},
            {"2022-03-15", "2022-03-18"},
            {"2022-03-08", "2022-03-11"},
            {"2022-03-01", "2022-03-04"},
            {"2022-02-22", "2022-02-25"},
            {"2022-02-15", "2022-02-18"},
            {"2022-02-08", "2022-02-11"},
            {"2022-02-01", "2022-02-04"},
            {"2022-01-25", "2022-01-28"},
            {"2022-01-18", "2022-01-21"},
            {"2022-01-11", "2022-01-14"},
            {"2022-01-04", "2022-01-07"},
            {"2021-12-28", "2022-01-03"},
            {"2021-12-21", "2021-12-27"},
            {"2021-12-14", "2021-12-17"},
            {"2021-12-07", "2021-12-10"},
            {"2021-11-30", "2021-12-03"},
            {"2021-11-23", "2021-11-29"},
            {"2021-11-16", "2021-11-19"},
            {"2021-11-09", "2021-11-15"},
            {"2021-11-02", "2021-11-05"},
            {"2021-10-26", "2021-10-29"},
            {"2021-10-19", "2021-10-22"},
            {"2021-10-12", "2021-10-15"},
            {"2021-10-05", "2021-10-08"},
            {"2021-09-28", "2021-10-01"},
            {"2021-09-21", "2021-09-24"},
            {"2021-09-14", "2021-09-17"},
            {"2021-09-07", "2021-09-10"},
            {"2021-08-31", "2021-09-03"},
            {"2021-08-24", "2021-08-27"},
            {"2021-08-17", "2021-08-20"},
            {"2021-08-10", "2021-08-13"},
            {"2021-08-03", "2021-08-06"},
            {"2021-07-27", "2021-07-30"},
            {"2021-07-20", "2021-07-23"},
            {"2021-07-13", "2021-07-16"},
            {"2021-07-06", "2021-07-09"},
            {"2021-06-29", "2021-07-02"},
            {"2021-06-22", "2021-06-25"},
            {"2021-06-15", "2021-06-18"},
            {"2021-06-08", "2021-06-11"},
            {"2021-06-01", "2021-06-04"},
            {"2021-05-25", "2021-05-28"},
            {"2021-05-18", "2021-05-21"},
            {"2021-05-11", "2021-05-14"},
            {"2021-05-04", "2021-05-07"},
            {"2021-04-27", "2021-04-30"},
            {"2021-04-20", "2021-04-23"},
            {"2021-04-13", "2021-04-16"},
            {"2021-04-06", "2021-04-09"},
            {"2021-03-30", "2021-04-02"},
            {"2021-03-23", "2021-03-26"},
            {"2021-03-16", "2021-03-19"},
            {"2021-03-09", "2021-03-12"},
            {"2021-03-02", "2021-03-05"},
            {"2021-02-23", "2021-02-26"},
            {"2021-02-16", "2021-02-19"},
            {"2021-02-09", "2021-02-12"},
            {"2021-02-02", "2021-02-05"},
            {"2021-01-26", "2021-01-29"},
            {"2021-01-19", "2021-01-22"},
            {"2021-01-12", "2021-01-15"},
            {"2021-01-05", "2021-01-08"},
            {"2020-12-29", "2021-01-04"},
            {"2020-12-21", "2020-12-28"},
            {"2020-12-15", "2020-12-18"},
            {"2020-12-08", "2020-12-11"},
            {"2020-12-01", "2020-12-04"},
            {"2020-11-24", "2020-11-30"},
            {"2020-11-17", "2020-11-20"},
            {"2020-11-10", "2020-11-16"},
            {"2020-11-03", "2020-11-06"},
            {"2020-10-27", "2020-10-30"},
            {"2020-10-20", "2020-10-23"},
            {"2020-10-13", "2020-10-16"},
            {"2020-10-06", "2020-10-09"},
            {"2020-09-29", "2020-10-02"},
            {"2020-09-22", "2020-09-25"},
            {"2020-09-15", "2020-09-18"},
            {"2020-09-08", "2020-09-11"},
            {"2020-09-01", "2020-09-04"},
            {"2020-08-25", "2020-08-28"},
            {"2020-08-18", "2020-08-21"},
            {"2020-08-11", "2020-08-14"},
            {"2020-08-04", "2020-08-07"},
            {"2020-07-28", "2020-07-31"},
            {"2020-07-21", "2020-07-24"},
            {"2020-07-14", "2020-07-17"},
            {"2020-07-07", "2020-07-10"},
            {"2020-06-30", "2020-07-06"},
            {"2020-06-23", "2020-06-26"},
            {"2020-06-16", "2020-06-19"},
            {"2020-06-09", "2020-06-12"},
            {"2020-06-02", "2020-06-05"},
            {"2020-05-26", "2020-05-29"},
            {"2020-05-19", "2020-05-22"},
            {"2020-05-12", "2020-05-15"},
            {"2020-05-05", "2020-05-08"},
            {"2020-04-28", "2020-05-01"},
            {"2020-04-21", "2020-04-24"},
            {"2020-04-14", "2020-04-17"},
            {"2020-04-07", "2020-04-10"},
            {"2020-03-31", "2020-04-03"},
            {"2020-03-24", "2020-03-27"},
            {"2020-03-17", "2020-03-20"},
            {"2020-03-10", "2020-03-13"},
            {"2020-03-03", "2020-03-06"},
            {"2020-02-25", "2020-02-28"},
            {"2020-02-18", "2020-02-21"},
            {"2020-02-11", "2020-02-14"},
            {"2020-02-04", "2020-02-07"},
            {"2020-01-28", "2020-01-31"},
            {"2020-01-21", "2020-01-24"},
            {"2020-01-14", "2020-01-17"},
            {"2020-01-07", "2020-01-10"},
            {"2019-12-31", "2020-01-06"},
            {"2019-12-24", "2019-12-30"},
            {"2019-12-17", "2019-12-20"},
            {"2019-12-10", "2019-12-13"},
            {"2019-12-03", "2019-12-06"},
            {"2019-11-26", "2019-12-02"},
            {"2019-11-19", "2019-11-22"},
            {"2019-11-12", "2019-11-15"},
            {"2019-11-05", "2019-11-08"},
            {"2019-10-29", "2019-11-01"},
            {"2019-10-22", "2019-10-25"},
            {"2019-10-15", "2019-10-18"},
            {"2019-10-08", "2019-10-11"},
            {"2019-10-01", "2019-10-04"},
            {"2019-09-24", "2019-09-27"},
            {"2019-09-17", "2019-09-20"},
            {"2019-09-10", "2019-09-13"},
            {"2019-09-03", "2019-09-06"},
            {"2019-08-27", "2019-08-30"},
            {"2019-08-20", "2019-08-23"},
            {"2019-08-13", "2019-08-16"},
            {"2019-08-06", "2019-08-09"},
            {"2019-07-30", "2019-08-02"},
            {"2019-07-23", "2019-07-26"},
            {"2019-07-16", "2019-07-19"},
            {"2019-07-09", "2019-07-12"},
            {"2019-07-02", "2019-07-08"},
            {"2019-06-25", "2019-06-28"},
            {"2019-06-18", "2019-06-21"},
            {"2019-06-11", "2019-06-14"},
            {"2019-06-04", "2019-06-07"},
            {"2019-05-28", "2019-05-31"},
            {"2019-05-21", "2019-05-24"},
            {"2019-05-14", "2019-05-17"},
            {"2019-05-07", "2019-05-10"},
            {"2019-04-30", "2019-05-03"},
            {"2019-04-23", "2019-04-26"},
            {"2019-04-16", "2019-04-19"},
            {"2019-04-09", "2019-04-12"},
            {"2019-04-02", "2019-04-05"},
            {"2019-03-26", "2019-03-29"},
            {"2019-03-19", "2019-03-22"},
            {"2019-03-12", "2019-03-15"},
            {"2019-03-05", "2019-03-08"},
            {"2019-02-26", "2019-03-01"},
            {"2019-02-19", "2019-02-22"},
            {"2019-02-12", "2019-02-15"},
            {"2019-02-05", "2019-02-08"},
            {"2019-01-29", "2019-02-01"},
            {"2019-01-22", "2019-01-25"},
            {"2019-01-15", "2019-01-18"},
            {"2019-01-08", "2019-01-11"},
            {"2018-12-31", "2019-01-04"},
            {"2018-12-24", "2018-12-28"},
            {"2018-12-18", "2018-12-21"},
            {"2018-12-11", "2018-12-14"},
            {"2018-12-04", "2018-12-07"},
            {"2018-11-27", "2018-11-30"},
            {"2018-11-20", "2018-11-26"},
            {"2018-11-13", "2018-11-16"},
            {"2018-11-06", "2018-11-09"},
            {"2018-10-30", "2018-11-02"},
            {"2018-10-23", "2018-10-26"},
            {"2018-10-16", "2018-10-19"},
            {"2018-10-09", "2018-10-12"},
            {"2018-10-02", "2018-10-05"},
            {"2018-09-25", "2018-09-28"},
            {"2018-09-18", "2018-09-21"},
            {"2018-09-11", "2018-09-14"},
            {"2018-09-04", "2018-09-07"},
            {"2018-08-28", "2018-08-31"},
            {"2018-08-21", "2018-08-24"},
            {"2018-08-14", "2018-08-17"},
            {"2018-08-07", "2018-08-10"},
            {"2018-07-31", "2018-08-03"},
            {"2018-07-24", "2018-07-27"},
            {"2018-07-17", "2018-07-20"},
            {"2018-07-10", "2018-07-13"},
            {"2018-07-03", "2018-07-09"},
            {"2018-06-26", "2018-06-29"},
            {"2018-06-19", "2018-06-22"},
            {"2018-06-12", "2018-06-15"},
            {"2018-06-05", "2018-06-08"},
            {"2018-05-29", "2018-06-01"},
            {"2018-05-22", "2018-05-25"},
            {"2018-05-15", "2018-05-18"},
            {"2018-05-08", "2018-05-11"},
            {"2018-05-01", "2018-05-04"},
            {"2018-04-24", "2018-04-27"},
            {"2018-04-17", "2018-04-20"},
            {"2018-04-10", "2018-04-13"},
            {"2018-04-03", "2018-04-06"},
            {"2018-03-27", "2018-03-30"},
            {"2018-03-20", "2018-03-23"},
            {"2018-03-13", "2018-03-16"},
            {"2018-03-06", "2018-03-09"},
            {"2018-02-27", "2018-03-02"},
            {"2018-02-20", "2018-02-23"},
            {"2018-02-13", "2018-02-16"},
            {"2018-02-06", "2018-02-09"},
            {"2018-01-30", "2018-02-02"},
            {"2018-01-23", "2018-01-26"},
            {"2018-01-16", "2018-01-19"},
            {"2018-01-09", "2018-01-12"},
            {"2018-01-02", "2018-01-05"},
            {"2017-12-26", "2017-12-29"},
            {"2017-12-19", "2017-12-22"},
            {"2017-12-12", "2017-12-15"},
            {"2017-12-05", "2017-12-08"},
            {"2017-11-28", "2017-12-01"},
            {"2017-11-21", "2017-11-27"},
            {"2017-11-14", "2017-11-17"},
            {"2017-11-07", "2017-11-13"},
            {"2017-10-31", "2017-11-03"},
            {"2017-10-24", "2017-10-27"},
            {"2017-10-17", "2017-10-20"},
            {"2017-10-10", "2017-10-13"},
            {"2017-10-03", "2017-10-06"},
            {"2017-09-26", "2017-09-29"},
            {"2017-09-19", "2017-09-22"},
            {"2017-09-12", "2017-09-15"},
            {"2017-09-05", "2017-09-08"},
            {"2017-08-29", "2017-09-01"},
            {"2017-08-22", "2017-08-25"},
            {"2017-08-15", "2017-08-18"},
            {"2017-08-08", "2017-08-11"},
            {"2017-08-01", "2017-08-04"},
            {"2017-07-25", "2017-07-28"},
            {"2017-07-18", "2017-07-21"},
            {"2017-07-11", "2017-07-14"},
            {"2017-07-03", "2017-07-07"},
            {"2017-06-27", "2017-06-30"},
            {"2017-06-20", "2017-06-23"},
            {"2017-06-13", "2017-06-16"},
            {"2017-06-06", "2017-06-09"},
            {"2017-05-30", "2017-06-02"},
            {"2017-05-23", "2017-05-26"},
            {"2017-05-16", "2017-05-19"},
            {"2017-05-09", "2017-05-12"},
            {"2017-05-02", "2017-05-05"},
            {"2017-04-25", "2017-04-28"},
            {"2017-04-18", "2017-04-21"},
            {"2017-04-11", "2017-04-14"},
            {"2017-04-04", "2017-04-07"},
            {"2017-03-28", "2017-03-31"},
            {"2017-03-21", "2017-03-24"},
            {"2017-03-14", "2017-03-17"},
            {"2017-03-07", "2017-03-10"},
            {"2017-02-28", "2017-03-03"},
            {"2017-02-21", "2017-02-24"},
            {"2017-02-14", "2017-02-17"},
            {"2017-02-07", "2017-02-10"},
            {"2017-01-31", "2017-02-03"},
            {"2017-01-24", "2017-01-27"},
            {"2017-01-17", "2017-01-20"},
            {"2017-01-10", "2017-01-13"},
            {"2017-01-03", "2017-01-06"},
            {"2016-12-27", "2016-12-30"},
            {"2016-12-20", "2016-12-23"},
            {"2016-12-13", "2016-12-16"},
            {"2016-12-06", "2016-12-09"},
            {"2016-11-29", "2016-12-02"},
            {"2016-11-22", "2016-11-28"},
            {"2016-11-15", "2016-11-18"},
            {"2016-11-08", "2016-11-14"},
            {"2016-11-01", "2016-11-04"},
            {"2016-10-25", "2016-10-28"},
            {"2016-10-18", "2016-10-21"},
            {"2016-10-11", "2016-10-14"},
            {"2016-10-04", "2016-10-07"},
            {"2016-09-27", "2016-09-30"},
            {"2016-09-20", "2016-09-23"},
            {"2016-09-13", "2016-09-16"},
            {"2016-09-06", "2016-09-09"},
            {"2016-08-30", "2016-09-02"},
            {"2016-08-23", "2016-08-26"},
            {"2016-08-16", "2016-08-19"},
            {"2016-08-09", "2016-08-12"},
            {"2016-08-02", "2016-08-05"},
            {"2016-07-26", "2016-07-29"},
            {"2016-07-19", "2016-07-22"},
            {"2016-07-12", "2016-07-15"},
            {"2016-07-05", "2016-07-08"},
            {"2016-06-28", "2016-07-01"},
            {"2016-06-21", "2016-06-24"},
            {"2016-06-14", "2016-06-17"},
            {"2016-06-07", "2016-06-10"},
            {"2016-05-31", "2016-06-03"},
            {"2016-05-24", "2016-05-27"},
            {"2016-05-17", "2016-05-20"},
            {"2016-05-10", "2016-05-13"},
            {"2016-05-03", "2016-05-06"},
            {"2016-04-26", "2016-04-29"},
            {"2016-04-19", "2016-04-22"},
            {"2016-04-12", "2016-04-15"},
            {"2016-04-05", "2016-04-08"},
            {"2016-03-29", "2016-04-01"},
            {"2016-03-22", "2016-03-25"},
            {"2016-03-15", "2016-03-18"},
            {"2016-03-08", "2016-03-11"},
            {"2016-03-01", "2016-03-04"},
            {"2016-02-23", "2016-02-26"},
            {"2016-02-16", "2016-02-19"},
            {"2016-02-09", "2016-02-12"},
            {"2016-02-02", "2016-02-05"},
            {"2016-01-26", "2016-01-29"},
            {"2016-01-19", "2016-01-22"},
            {"2016-01-12", "2016-01-15"},
            {"2016-01-05", "2016-01-08"},
            {"2015-12-29", "2016-01-04"},
            {"2015-12-22", "2015-12-28"},
            {"2015-12-15", "2015-12-18"},
            {"2015-12-08", "2015-12-11"},
            {"2015-12-01", "2015-12-04"},
            {"2015-11-24", "2015-11-30"},
            {"2015-11-17", "2015-11-20"},
            {"2015-11-10", "2015-11-13"},
            {"2015-11-03", "2015-11-06"},
            {"2015-10-27", "2015-10-30"},
            {"2015-10-20", "2015-10-23"},
            {"2015-10-13", "2015-10-16"},
            {"2015-10-06", "2015-10-09"},
            {"2015-09-29", "2015-10-02"},
            {"2015-09-22", "2015-09-25"},
            {"2015-09-15", "2015-09-18"},
            {"2015-09-08", "2015-09-11"},
            {"2015-09-01", "2015-09-04"},
            {"2015-08-25", "2015-08-28"},
            {"2015-08-18", "2015-08-21"},
            {"2015-08-11", "2015-08-14"},
            {"2015-08-04", "2015-08-07"},
            {"2015-07-28", "2015-07-31"},
            {"2015-07-21", "2015-07-24"},
            {"2015-07-14", "2015-07-17"},
            {"2015-07-07", "2015-07-10"},
            {"2015-06-30", "2015-07-06"},
            {"2015-06-23", "2015-06-26"},
            {"2015-06-16", "2015-06-19"},
            {"2015-06-09", "2015-06-12"},
            {"2015-06-02", "2015-06-05"},
            {"2015-05-26", "2015-05-29"},
            {"2015-05-19", "2015-05-22"},
            {"2015-05-12", "2015-05-15"},
            {"2015-05-05", "2015-05-08"},
            {"2015-04-28", "2015-05-01"},
            {"2015-04-21", "2015-04-24"},
            {"2015-04-14", "2015-04-17"},
            {"2015-04-07", "2015-04-10"},
            {"2015-03-31", "2015-04-03"},
            {"2015-03-24", "2015-03-27"},
            {"2015-03-17", "2015-03-20"},
            {"2015-03-10", "2015-03-13"},
            {"2015-03-03", "2015-03-06"},
            {"2015-02-24", "2015-02-27"},
            {"2015-02-17", "2015-02-20"},
            {"2015-02-10", "2015-02-13"},
            {"2015-02-03", "2015-02-06"},
            {"2015-01-27", "2015-01-30"},
            {"2015-01-20", "2015-01-23"},
            {"2015-01-13", "2015-01-16"},
            {"2015-01-06", "2015-01-09"},
            {"2014-12-30", "2015-01-05"},
            {"2014-12-23", "2014-12-29"},
            {"2014-12-16", "2014-12-19"},
            {"2014-12-09", "2014-12-12"},
            {"2014-12-02", "2014-12-05"},
            {"2014-11-25", "2014-12-01"},
            {"2014-11-18", "2014-11-21"},
            {"2014-11-11", "2014-11-14"},
            {"2014-11-04", "2014-11-07"},
            {"2014-10-28", "2014-10-31"},
            {"2014-10-21", "2014-10-24"},
            {"2014-10-14", "2014-10-17"},
            {"2014-10-07", "2014-10-10"},
            {"2014-09-30", "2014-10-03"},
            {"2014-09-23", "2014-09-26"},
            {"2014-09-16", "2014-09-19"},
            {"2014-09-09", "2014-09-12"},
            {"2014-09-02", "2014-09-05"},
            {"2014-08-26", "2014-08-29"},
            {"2014-08-19", "2014-08-22"},
            {"2014-08-12", "2014-08-15"},
            {"2014-08-05", "2014-08-08"},
            {"2014-07-29", "2014-08-01"},
            {"2014-07-22", "2014-07-25"},
            {"2014-07-15", "2014-07-18"},
            {"2014-07-08", "2014-07-11"},
            {"2014-07-01", "2014-07-07"},
            {"2014-06-24", "2014-06-27"},
            {"2014-06-17", "2014-06-20"},
            {"2014-06-10", "2014-06-13"},
            {"2014-06-03", "2014-06-06"},
            {"2014-05-27", "2014-05-30"},
            {"2014-05-20", "2014-05-23"},
            {"2014-05-13", "2014-05-16"},
            {"2014-05-06", "2014-05-09"},
            {"2014-04-29", "2014-05-02"},
            {"2014-04-22", "2014-04-25"},
            {"2014-04-15", "2014-04-18"},
            {"2014-04-08", "2014-04-11"},
            {"2014-04-01", "2014-04-04"},
            {"2014-03-25", "2014-03-28"},
            {"2014-03-18", "2014-03-21"},
            {"2014-03-11", "2014-03-14"},
            {"2014-03-04", "2014-03-07"},
            {"2014-02-25", "2014-02-28"},
            {"2014-02-18", "2014-02-21"},
            {"2014-02-11", "2014-02-14"},
            {"2014-02-04", "2014-02-07"},
            {"2014-01-28", "2014-01-31"},
            {"2014-01-21", "2014-01-24"},
            {"2014-01-14", "2014-01-17"},
            {"2014-01-07", "2014-01-10"},
            {"2013-12-31", "2014-01-06"},
            {"2013-12-24", "2013-12-30"},
            {"2013-12-17", "2013-12-20"},
            {"2013-12-10", "2013-12-13"},
            {"2013-12-03", "2013-12-06"},
            {"2013-11-26", "2013-12-02"},
            {"2013-11-19", "2013-11-22"},
            {"2013-11-12", "2013-11-15"},
            {"2013-11-05", "2013-11-08"},
            {"2013-10-29", "2013-11-01"},
            {"2013-10-22", "2013-10-25"},
            {"2013-10-15", "2013-10-18"},
            {"2013-10-08", "2013-10-11"},
            {"2013-10-01", "2013-10-04"},
            {"2013-09-24", "2013-09-27"},
            {"2013-09-17", "2013-09-20"},
            {"2013-09-10", "2013-09-13"},
            {"2013-09-03", "2013-09-06"},
            {"2013-08-27", "2013-08-30"},
            {"2013-08-20", "2013-08-23"},
            {"2013-08-13", "2013-08-16"},
            {"2013-08-06", "2013-08-09"},
            {"2013-07-30", "2013-08-02"},
            {"2013-07-23", "2013-07-26"},
            {"2013-07-16", "2013-07-19"},
            {"2013-07-09", "2013-07-12"},
            {"2013-07-02", "2013-07-08"},
            {"2013-06-25", "2013-06-28"},
            {"2013-06-18", "2013-06-21"},
            {"2013-06-11", "2013-06-14"},
            {"2013-06-04", "2013-06-07"},
            {"2013-05-28", "2013-05-31"},
            {"2013-05-21", "2013-05-24"},
            {"2013-05-14", "2013-05-17"},
            {"2013-05-07", "2013-05-10"},
            {"2013-04-30", "2013-05-03"},
            {"2013-04-23", "2013-04-26"},
            {"2013-04-16", "2013-04-19"},
            {"2013-04-09", "2013-04-12"},
            {"2013-04-02", "2013-04-05"},
            {"2013-03-26", "2013-03-29"},
            {"2013-03-19", "2013-03-22"},
            {"2013-03-12", "2013-03-15"},
            {"2013-03-05", "2013-03-08"},
            {"2013-02-26", "2013-03-01"},
            {"2013-02-19", "2013-02-22"},
            {"2013-02-12", "2013-02-15"},
            {"2013-02-05", "2013-02-08"},
            {"2013-01-29", "2013-02-01"},
            {"2013-01-22", "2013-01-25"},
            {"2013-01-15", "2013-01-18"},
            {"2013-01-08", "2013-01-11"},
            {"2012-12-31", "2013-01-04"},
            {"2012-12-24", "2012-12-28"},
            {"2012-12-18", "2012-12-21"},
            {"2012-12-11", "2012-12-14"},
            {"2012-12-04", "2012-12-07"},
            {"2012-11-27", "2012-11-30"},
            {"2012-11-20", "2012-11-26"},
            {"2012-11-13", "2012-11-16"},
            {"2012-11-06", "2012-11-09"},
            {"2012-10-30", "2012-11-02"},
            {"2012-10-23", "2012-10-26"},
            {"2012-10-16", "2012-10-19"},
            {"2012-10-09", "2012-10-12"},
            {"2012-10-02", "2012-10-05"},
            {"2012-09-25", "2012-09-28"},
            {"2012-09-18", "2012-09-21"},
            {"2012-09-11", "2012-09-14"},
            {"2012-09-04", "2012-09-07"},
            {"2012-08-28", "2012-08-31"},
            {"2012-08-21", "2012-08-24"},
            {"2012-08-14", "2012-08-17"},
            {"2012-08-07", "2012-08-10"},
            {"2012-07-31", "2012-08-03"},
            {"2012-07-24", "2012-07-27"},
            {"2012-07-17", "2012-07-20"},
            {"2012-07-10", "2012-07-13"},
            {"2012-07-03", "2012-07-09"},
            {"2012-06-26", "2012-06-29"},
            {"2012-06-19", "2012-06-22"},
            {"2012-06-12", "2012-06-15"},
            {"2012-06-05", "2012-06-08"},
            {"2012-05-29", "2012-06-01"},
            {"2012-05-22", "2012-05-25"},
            {"2012-05-15", "2012-05-18"},
            {"2012-05-08", "2012-05-11"},
            {"2012-05-01", "2012-05-04"},
            {"2012-04-24", "2012-04-27"},
            {"2012-04-17", "2012-04-20"},
            {"2012-04-10", "2012-04-13"},
            {"2012-04-03", "2012-04-06"},
            {"2012-03-27", "2012-03-30"},
            {"2012-03-20", "2012-03-23"},
            {"2012-03-13", "2012-03-16"},
            {"2012-03-06", "2012-03-09"},
            {"2012-02-28", "2012-03-02"},
            {"2012-02-21", "2012-02-24"},
            {"2012-02-14", "2012-02-17"},
            {"2012-02-07", "2012-02-10"},
            {"2012-01-31", "2012-02-03"},
            {"2012-01-24", "2012-01-27"},
            {"2012-01-17", "2012-01-20"},
            {"2012-01-10", "2012-01-13"},
            {"2012-01-03", "2012-01-06"},
            {"2011-12-27", "2011-12-30"},
            {"2011-12-20", "2011-12-23"},
            {"2011-12-13", "2011-12-16"},
            {"2011-12-06", "2011-12-09"},
            {"2011-11-29", "2011-12-02"},
            {"2011-11-22", "2011-11-28"},
            {"2011-11-15", "2011-11-18"},
            {"2011-11-08", "2011-11-14"},
            {"2011-11-01", "2011-11-04"},
            {"2011-10-25", "2011-10-28"},
            {"2011-10-18", "2011-10-21"},
            {"2011-10-11", "2011-10-14"},
            {"2011-10-04", "2011-10-07"},
            {"2011-09-27", "2011-09-30"},
            {"2011-09-20", "2011-09-23"},
            {"2011-09-13", "2011-09-16"},
            {"2011-09-06", "2011-09-09"},
            {"2011-08-30", "2011-09-02"},
            {"2011-08-23", "2011-08-26"},
            {"2011-08-16", "2011-08-19"},
            {"2011-08-09", "2011-08-12"},
            {"2011-08-02", "2011-08-05"},
            {"2011-07-26", "2011-07-29"},
            {"2011-07-19", "2011-07-22"},
            {"2011-07-12", "2011-07-15"},
            {"2011-07-05", "2011-07-08"},
            {"2011-06-28", "2011-07-01"},
            {"2011-06-21", "2011-06-24"},
            {"2011-06-14", "2011-06-17"},
            {"2011-06-07", "2011-06-10"},
            {"2011-05-31", "2011-06-03"},
            {"2011-05-24", "2011-05-27"},
            {"2011-05-17", "2011-05-20"},
            {"2011-05-10", "2011-05-13"},
            {"2011-05-03", "2011-05-06"},
            {"2011-04-26", "2011-04-29"},
            {"2011-04-19", "2011-04-22"},
            {"2011-04-12", "2011-04-15"},
            {"2011-04-05", "2011-04-08"},
            {"2011-03-29", "2011-04-01"},
            {"2011-03-22", "2011-03-25"},
            {"2011-03-15", "2011-03-18"},
            {"2011-03-08", "2011-03-11"},
            {"2011-03-01", "2011-03-04"},
            {"2011-02-22", "2011-02-25"},
            {"2011-02-15", "2011-02-18"},
            {"2011-02-08", "2011-02-11"},
            {"2011-02-01", "2011-02-04"},
            {"2011-01-25", "2011-01-28"},
            {"2011-01-18", "2011-01-21"},
            {"2011-01-11", "2011-01-14"},
            {"2011-01-04", "2011-01-07"},
            {"2010-12-28", "2011-01-03"},
            {"2010-12-21", "2010-12-27"},
            {"2010-12-14", "2010-12-17"},
            {"2010-12-07", "2010-12-10"},
            {"2010-11-30", "2010-12-03"},
            {"2010-11-23", "2010-11-29"},
            {"2010-11-16", "2010-11-19"},
            {"2010-11-09", "2010-11-15"},
            {"2010-11-02", "2010-11-05"},
            {"2010-10-26", "2010-10-29"},
            {"2010-10-19", "2010-10-22"},
            {"2010-10-12", "2010-10-15"},
            {"2010-10-05", "2010-10-08"},
            {"2010-09-28", "2010-10-01"},
            {"2010-09-21", "2010-09-24"},
            {"2010-09-14", "2010-09-17"},
            {"2010-09-07", "2010-09-10"},
            {"2010-08-31", "2010-09-03"},
            {"2010-08-24", "2010-08-27"},
            {"2010-08-17", "2010-08-20"},
            {"2010-08-10", "2010-08-13"},
            {"2010-08-03", "2010-08-06"},
            {"2010-07-27", "2010-07-30"},
            {"2010-07-20", "2010-07-23"},
            {"2010-07-13", "2010-07-16"},
            {"2010-07-06", "2010-07-09"},
            {"2010-06-29", "2010-07-02"},
            {"2010-06-22", "2010-06-25"},
            {"2010-06-15", "2010-06-18"},
            {"2010-06-08", "2010-06-11"},
            {"2010-06-01", "2010-06-04"},
            {"2010-05-25", "2010-05-28"},
            {"2010-05-18", "2010-05-21"},
            {"2010-05-11", "2010-05-14"},
            {"2010-05-04", "2010-05-07"},
            {"2010-04-27", "2010-04-30"},
            {"2010-04-20", "2010-04-23"},
            {"2010-04-13", "2010-04-16"},
            {"2010-04-06", "2010-04-09"},
            {"2010-03-30", "2010-04-02"},
            {"2010-03-23", "2010-03-26"},
            {"2010-03-16", "2010-03-19"},
            {"2010-03-09", "2010-03-12"},
            {"2010-03-02", "2010-03-05"},
            {"2010-02-23", "2010-02-26"},
            {"2010-02-16", "2010-02-19"},
            {"2010-02-09", "2010-02-12"},
            {"2010-02-02", "2010-02-05"},
            {"2010-01-26", "2010-01-29"},
            {"2010-01-19", "2010-01-22"},
            {"2010-01-12", "2010-01-15"},
            {"2010-01-05", "2010-01-08"},
            {"2009-12-29", "2010-01-04"},
            {"2009-12-22", "2009-12-28"},
            {"2009-12-15", "2009-12-18"},
            {"2009-12-08", "2009-12-11"},
            {"2009-12-01", "2009-12-04"},
            {"2009-11-24", "2009-11-30"},
            {"2009-11-17", "2009-11-20"},
            {"2009-11-09", "2009-11-13"},
            {"2009-11-03", "2009-11-06"},
            {"2009-10-27", "2009-10-30"},
            {"2009-10-20", "2009-10-23"},
            {"2009-10-13", "2009-10-16"},
            {"2009-10-06", "2009-10-09"},
            {"2009-09-29", "2009-10-02"},
            {"2009-09-22", "2009-09-25"},
            {"2009-09-15", "2009-09-18"},
            {"2009-09-08", "2009-09-11"},
            {"2009-09-01", "2009-09-04"},
            {"2009-08-25", "2009-08-28"},
            {"2009-08-18", "2009-08-21"},
            {"2009-08-11", "2009-08-14"},
            {"2009-08-04", "2009-08-07"},
            {"2009-07-28", "2009-07-31"},
            {"2009-07-21", "2009-07-24"},
            {"2009-07-14", "2009-07-17"},
            {"2009-07-07", "2009-07-10"},
            {"2009-06-30", "2009-07-06"},
            {"2009-06-23", "2009-06-26"},
            {"2009-06-16", "2009-06-19"},
            {"2009-06-09", "2009-06-12"},
            {"2009-06-02", "2009-06-05"},
            {"2009-05-26", "2009-05-29"},
            {"2009-05-19", "2009-05-22"},
            {"2009-05-12", "2009-05-15"},
            {"2009-05-05", "2009-05-08"},
            {"2009-04-28", "2009-05-01"},
            {"2009-04-21", "2009-04-24"},
            {"2009-04-14", "2009-04-17"},
            {"2009-04-07", "2009-04-10"},
            {"2009-03-31", "2009-04-03"},
            {"2009-03-24", "2009-03-27"},
            {"2009-03-17", "2009-03-20"},
            {"2009-03-10", "2009-03-13"},
            {"2009-03-03", "2009-03-06"},
            {"2009-02-24", "2009-02-27"},
            {"2009-02-17", "2009-02-20"},
            {"2009-02-10", "2009-02-13"},
            {"2009-02-03", "2009-02-06"},
            {"2009-01-27", "2009-01-30"},
            {"2009-01-20", "2009-01-23"},
            {"2009-01-13", "2009-01-16"},
            {"2009-01-06", "2009-01-09"},
            {"2008-12-30", "2009-01-05"},
            {"2008-12-22", "2008-12-29"},
            {"2008-12-16", "2008-12-19"},
            {"2008-12-09", "2008-12-12"},
            {"2008-12-02", "2008-12-05"},
            {"2008-11-25", "2008-12-01"},
            {"2008-11-18", "2008-11-21"},
            {"2008-11-11", "2008-11-14"},
            {"2008-11-04", "2008-11-07"},
            {"2008-10-28", "2008-10-31"},
            {"2008-10-21", "2008-10-24"},
            {"2008-10-14", "2008-10-17"},
            {"2008-10-07", "2008-10-10"},
            {"2008-09-30", "2008-10-03"},
            {"2008-09-23", "2008-09-26"},
            {"2008-09-16", "2008-09-19"},
            {"2008-09-09", "2008-09-12"},
            {"2008-09-02", "2008-09-05"},
            {"2008-08-26", "2008-08-29"},
            {"2008-08-19", "2008-08-22"},
            {"2008-08-12", "2008-08-15"},
            {"2008-08-05", "2008-08-08"},
            {"2008-07-29", "2008-08-01"},
            {"2008-07-22", "2008-07-25"},
            {"2008-07-15", "2008-07-18"},
            {"2008-07-08", "2008-07-11"},
            {"2008-07-01", "2008-07-07"},
            {"2008-06-24", "2008-06-27"},
            {"2008-06-17", "2008-06-20"},
            {"2008-06-10", "2008-06-13"},
            {"2008-06-03", "2008-06-06"},
            {"2008-05-27", "2008-05-30"},
            {"2008-05-20", "2008-05-23"},
            {"2008-05-13", "2008-05-16"},
            {"2008-05-06", "2008-05-09"},
            {"2008-04-29", "2008-05-02"},
            {"2008-04-22", "2008-04-25"},
            {"2008-04-15", "2008-04-18"},
            {"2008-04-08", "2008-04-11"},
            {"2008-04-01", "2008-04-04"},
            {"2008-03-25", "2008-03-28"},
            {"2008-03-18", "2008-03-21"},
            {"2008-03-11", "2008-03-14"},
            {"2008-03-04", "2008-03-07"},
            {"2008-02-26", "2008-02-29"},
            {"2008-02-19", "2008-02-22"},
            {"2008-02-12", "2008-02-15"},
            {"2008-02-05", "2008-02-08"},
            {"2008-01-29", "2008-02-01"},
            {"2008-01-22", "2008-01-25"},
            {"2008-01-15", "2008-01-18"},
            {"2008-01-08", "2008-01-11"},
            {"2007-12-31", "2008-01-04"},
            {"2007-12-24", "2007-12-28"},
            {"2007-12-18", "2007-12-21"},
            {"2007-12-11", "2007-12-14"},
            {"2007-12-04", "2007-12-07"},
            {"2007-11-27", "2007-11-30"},
            {"2007-11-20", "2007-11-26"},
            {"2007-11-13", "2007-11-16"},
            {"2007-11-06", "2007-11-09"},
            {"2007-10-30", "2007-11-02"},
            {"2007-10-23", "2007-10-26"},
            {"2007-10-16", "2007-10-19"},
            {"2007-10-09", "2007-10-12"},
            {"2007-10-02", "2007-10-05"},
            {"2007-09-25", "2007-09-28"},
            {"2007-09-18", "2007-09-21"},
            {"2007-09-11", "2007-09-14"},
            {"2007-09-04", "2007-09-07"},
            {"2007-08-28", "2007-08-31"},
            {"2007-08-21", "2007-08-24"},
            {"2007-08-14", "2007-08-17"},
            {"2007-08-07", "2007-08-10"},
            {"2007-07-31", "2007-08-03"},
            {"2007-07-24", "2007-07-27"},
            {"2007-07-17", "2007-07-20"},
            {"2007-07-10", "2007-07-13"},
            {"2007-07-03", "2007-07-09"},
            {"2007-06-26", "2007-06-29"},
            {"2007-06-19", "2007-06-22"},
            {"2007-06-12", "2007-06-15"},
            {"2007-06-05", "2007-06-08"},
            {"2007-05-29", "2007-06-01"},
            {"2007-05-22", "2007-05-25"},
            {"2007-05-15", "2007-05-18"},
            {"2007-05-08", "2007-05-11"},
            {"2007-05-01", "2007-05-04"},
            {"2007-04-24", "2007-04-27"},
            {"2007-04-17", "2007-04-20"},
            {"2007-04-10", "2007-04-13"},
            {"2007-04-03", "2007-04-06"},
            {"2007-03-27", "2007-03-30"},
            {"2007-03-20", "2007-03-23"},
            {"2007-03-13", "2007-03-16"},
            {"2007-03-06", "2007-03-09"},
            {"2007-02-27", "2007-03-02"},
            {"2007-02-20", "2007-02-23"},
            {"2007-02-13", "2007-02-16"},
            {"2007-02-06", "2007-02-09"},
            {"2007-01-30", "2007-02-02"},
            {"2007-01-23", "2007-01-26"},
            {"2007-01-16", "2007-01-19"},
            {"2007-01-09", "2007-01-12"},
            {"2007-01-03", "2007-01-08"},
            {"2006-12-26", "2006-12-29"},
            {"2006-12-19", "2006-12-22"},
            {"2006-12-12", "2006-12-15"},
            {"2006-12-05", "2006-12-08"},
            {"2006-11-28", "2006-12-01"},
            {"2006-11-21", "2006-11-27"},
            {"2006-11-14", "2006-11-17"},
            {"2006-11-07", "2006-11-13"},
            {"2006-10-31", "2006-11-03"},
            {"2006-10-24", "2006-10-27"},
            {"2006-10-17", "2006-10-20"},
            {"2006-10-10", "2006-10-13"},
            {"2006-10-03", "2006-10-06"},
            {"2006-09-26", "2006-09-29"},
            {"2006-09-19", "2006-09-22"},
            {"2006-09-12", "2006-09-15"},
            {"2006-09-05", "2006-09-08"},
            {"2006-08-29", "2006-09-01"},
            {"2006-08-22", "2006-08-25"},
            {"2006-08-15", "2006-08-18"},
            {"2006-08-08", "2006-08-11"},
            {"2006-08-01", "2006-08-04"},
            {"2006-07-25", "2006-07-28"},
            {"2006-07-18", "2006-07-21"},
            {"2006-07-11", "2006-07-14"},
            {"2006-07-03", "2006-07-07"},
            {"2006-06-27", "2006-06-30"},
            {"2006-06-20", "2006-06-23"},
            {"2006-06-13", "2006-06-16"},
            {"2006-06-06", "2006-06-09"},
            {"2006-05-30", "2006-06-02"},
            {"2006-05-23", "2006-05-26"},
            {"2006-05-16", "2006-05-19"},
            {"2006-05-09", "2006-05-12"},
            {"2006-05-02", "2006-05-05"},
            {"2006-04-25", "2006-04-28"},
            {"2006-04-18", "2006-04-21"},
            {"2006-04-11", "2006-04-14"},
            {"2006-04-04", "2006-04-07"},
            {"2006-03-28", "2006-03-31"},
            {"2006-03-21", "2006-03-24"},
            {"2006-03-14", "2006-03-17"},
            {"2006-03-07", "2006-03-10"},
            {"2006-02-28", "2006-03-03"},
            {"2006-02-21", "2006-02-24"},
            {"2006-02-14", "2006-02-17"},
            {"2006-02-07", "2006-02-10"},
            {"2006-01-31", "2006-02-03"},
            {"2006-01-24", "2006-01-27"},
            {"2006-01-17", "2006-01-20"},
            {"2006-01-10", "2006-01-13"},
            {"2006-01-03", "2006-01-06"},
            {"2005-12-27", "2005-12-30"},
            {"2005-12-20", "2005-12-23"},
            {"2005-12-13", "2005-12-16"},
            {"2005-12-06", "2005-12-09"},
            {"2005-11-29", "2005-12-02"},
            {"2005-11-22", "2005-11-28"},
            {"2005-11-15", "2005-11-18"},
            {"2005-11-08", "2005-11-14"},
            {"2005-11-01", "2005-11-04"},
            {"2005-10-25", "2005-10-28"},
            {"2005-10-18", "2005-10-21"},
            {"2005-10-11", "2005-10-14"},
            {"2005-10-04", "2005-10-07"},
            {"2005-09-27", "2005-09-30"},
            {"2005-09-20", "2005-09-23"},
            {"2005-09-13", "2005-09-16"},
            {"2005-09-06", "2005-09-09"},
            {"2005-08-30", "2005-09-02"},
            {"2005-08-23", "2005-08-26"},
            {"2005-08-16", "2005-08-19"},
            {"2005-08-09", "2005-08-12"},
            {"2005-08-02", "2005-08-05"},
            {"2005-07-26", "2005-07-29"},
            {"2005-07-19", "2005-07-22"},
            {"2005-07-12", "2005-07-15"},
            {"2005-07-05", "2005-07-08"},
            {"2005-06-28", "2005-07-01"},
            {"2005-06-21", "2005-06-24"},
            {"2005-06-14", "2005-06-17"},
            {"2005-06-07", "2005-06-10"},
            {"2005-05-31", "2005-06-03"},
            {"2005-05-24", "2005-05-27"},
            {"2005-05-17", "2005-05-20"},
            {"2005-05-10", "2005-05-13"},
            {"2005-05-03", "2005-05-06"},
            {"2005-04-26", "2005-04-29"},
            {"2005-04-19", "2005-04-22"},
            {"2005-04-12", "2005-04-15"},
            {"2005-04-05", "2005-04-08"},
            {"2005-03-29", "2005-04-01"},
            {"2005-03-22", "2005-03-25"},
            {"2005-03-15", "2005-03-18"},
            {"2005-03-08", "2005-03-11"},
            {"2005-03-01", "2005-03-04"},
            {"2005-02-22", "2005-02-25"},
            {"2005-02-15", "2005-02-18"},
            {"2005-02-08", "2005-02-11"},
            {"2005-02-01", "2005-02-04"},
            {"2005-01-25", "2005-01-28"},
            {"2005-01-18", "2005-01-21"},
            {"2005-01-11", "2005-01-14"},
            {"2005-01-04", "2005-01-07"},
        };
    }
}