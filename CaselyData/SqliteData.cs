﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.IO;
using System.Data.SQLite;

namespace CaselyData {
    public class PathCase {
        public string CaseNumber { get; set; }
        public string Service { get; set; }
        public string Evaluation { get; set; }
    }
    public class CaseEntry {
        string result = "";
        string microscopic = "";
        string history = "";
        string material = "";
        string gross = "";
        string tumorSynoptic = "";
        string comment = "";
        public int Id { get; set; }
        public string CaseNumber { get; set; }
        public string Interpretation { get; set; }
        public string Result { get { return this.result; }
            set {
                this.result = value;
                // parse the result and fill in the micro,gross,material, and history fields.
                PopulateMicrGrossMatHist();
            }
        }
        public string Material { get { return material ; } set { material = value; } }
        public string History { get { return history; } set { history = value; } }
        public string Microscopic { get { return microscopic; } set { microscopic = value; } }
        public string Gross { get { return gross; }  set { gross = value; } }
        public string TumorSynoptic { get { return tumorSynoptic; } set { tumorSynoptic = value; } }
        public string Comment { get { return comment; } set { comment = value; } }
        public string TimeModifiedString { get; set; }
        public string DateModifiedString { get; set; }
        
        public DateTime DateTimeModifiedObject {
            get {
                return DateTime.Parse(DateModifiedString + " " + TimeModifiedString);
            }
            set {
                DateModifiedString = value.ToString("yyyy-MM-dd");

                TimeModifiedString = value.TimeOfDay.ToString();
            }
        }

        public string ShortDateModifiedString {
            get {
                return DateTimeModifiedObject.ToShortDateString();
            }
        }
        public string AuthorID { get; set; }
        public List<PartEntry> ListPartEntry { get; set; }

        public string toHtml() {
            string htmlCaseEntry = $"Author ID: {AuthorID}";
            htmlCaseEntry += $"<h3><u>Interpretation</u></h3>{Interpretation}";
            htmlCaseEntry += $"<h3><u>Material</u></h3>{Material}";
            htmlCaseEntry += $"<h3><u>History</u></h3>{History}";
            htmlCaseEntry += $"<h3><u>Gross</u></h3>{Gross}";
            htmlCaseEntry += $"<h3><u>Microscopic</u></h3>{Microscopic}";
            htmlCaseEntry += $"<h3><u>Tumor Synoptic</u></h3>{TumorSynoptic}";
            htmlCaseEntry += $"<h3><u>Comment</u></h3>{Comment}";
            htmlCaseEntry = "<head><style>P { white-space: pre; }</style></head>" + htmlCaseEntry;

            return htmlCaseEntry.Replace("\n","<br>");
        }
        private void PopulateMicrGrossMatHist() {
            List<string> sectionWords = new List<string> { "MATERIAL:", "HISTORY:", "GROSS:", "MICROSCOPIC:" };
            bool endOfResult = false;
            var linesPathResult = Result.Trim().Replace("\r", "").Split('\n');
            int i = 0;
            int lineCount = linesPathResult.Length;
            while (i < lineCount) {
                if (endOfResult) { break; } // we have reached the end of the result. this skips the microscopic dictation portion of report.
                var curLine = linesPathResult[i].Trim();
                var l = "";
                // Reads through result and adds the text from each section to the appropriate CaseEntry property.
                switch (curLine) {
                    case "MATERIAL:":
                        // continue adding text until next section is reached
                        do {
                            l = linesPathResult[++i];
                            Material += l + "\n";
                            if (i+1 >= lineCount) break; // handles the case if no other sections are found after material
                        } while (sectionWords.IndexOf(linesPathResult[i + 1].Trim()) == -1);
                        Material = material.Trim();
                        break;
                    case "HISTORY:":
                        do {
                            l = linesPathResult[++i];
                            History += l + "\n";
                            if (i + 1 >= lineCount) break;
                        } while (sectionWords.IndexOf(linesPathResult[i + 1].Trim()) == -1);
                        History = history.Trim();
                        break;
                    case "GROSS:":
                        do {
                            l = linesPathResult[++i];
                            Gross += l + "\n";
                            if (i + 1 >= lineCount) break;
                        } while (sectionWords.IndexOf(linesPathResult[i + 1].Trim()) == -1);
                        Gross = gross.Trim();
                        break;
                    case "MICROSCOPIC:":
                        // will continue parsing until the last line of the report is reached
                        do {
                            l = linesPathResult[++i];
                            Microscopic += l + "\n";
                            if (i + 1 >= lineCount) break;
                        } while (!(l.StartsWith("Microscopic Dictator ID")));
                        endOfResult = true;
                        Microscopic = microscopic.Trim();
                        break;
                    default:
                        // just in case nothing matches. Safety measure to at least continue to increment.
                        i++;
                        break;
                }
                i++;
            }
        }
    }

    public class PartEntry {
        public string Id { get; set; }
        public string CaseNumber { get; set; }
        public string Part { get; set; }
        public string Procedure { get; set; }
        public string Specimen { get; set; }
        public string TimeModifiedString { get; set; }
        public string DateModifiedString { get; set; }
        public string AuthorID { get; set; }
       
        public DateTime DateTimeModifiedObject {
            get {
                return DateTime.Parse(DateModifiedString + " " + TimeModifiedString);
            }
            set {
                DateModifiedString = value.ToString("yyyy-MM-dd");

                TimeModifiedString = value.ToString("HH:mm:ss");
            }
        }
    }

    public class PartDiagnosis {
        public string Id { get; set; }
        public string CaseNumber { get; set; }
        public string Part { get; set; }
        public string DateModifiedString { get; set; }
        public string TimeModifiedString { get; set; }
        public string OrganSystem { get; set; }
        public string Organ { get; set; }
        public string Category { get; set; }
        public string Diagnosis { get; set; }
        public string DiagnosisDetailed { get; set; }
        public DateTime DateTimeModifiedObject {
            get {
                return DateTime.Parse(DateModifiedString + " " + TimeModifiedString);
            }
            set {
                DateModifiedString = value.ToString("yyyy-MM-dd");

                TimeModifiedString = value.ToString("HH:mm:ss");
            }
        }
    }

    public class Staff {
        public string AuthorID { get; set; }
        public string FirstLastName { get; set; }
        public string Role { get; set; }
        public override string ToString() {
            return AuthorID;
        }
    }

    public class SqliteDataAcces {

        public static string CaseNumberPrefix = "SMP-18-";

        

        public static void InsertNewCaseEntry(CaseEntry ce, PathCase pathCase) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"INSERT INTO path_case (case_number, service)
                             VALUES (@CaseNumber, @Service);";
                cn.Execute(sql, pathCase);
                sql = @"INSERT INTO case_entry (author_id, case_number, date_modified,
                                                time_modified, tumor_synoptic, comment, result, material, history, interpretation, gross, microscopic)
                            VALUES (@AuthorID, @CaseNumber,@DateModifiedString,@TimeModifiedString,@TumorSynoptic, @Comment, @Result, @Material, @History, 
                                    @Interpretation, @Gross, @Microscopic);";
                cn.Execute(sql, ce);
            }
        }

        public static void BatchInsertNewCaseEntry(List<CaseEntry> listCasesToInsert) {

            using (var cn = new SQLiteConnection(DbConnectionString)) {
                cn.Open();
                cn.EnableExtensions(true);
                cn.LoadExtension("SQLite.Interop.dll", "sqlite3_fts5_init"); // this line is required to enable full text search support
                var sqliteTransaction = cn.BeginTransaction();

                foreach (var ce in listCasesToInsert) {
                    var sql = @"INSERT INTO path_case (case_number)
                                 VALUES (@CaseNumber);

                                INSERT INTO case_entry (author_id, case_number, date_modified,
                                       time_modified, tumor_synoptic, comment, result, material, history, interpretation, gross, microscopic)
                                VALUES (@AuthorID, @CaseNumber,@DateModifiedString,@TimeModifiedString,@TumorSynoptic, @Comment, @Result, @Material, @History, 
                                        @Interpretation, @Gross, @Microscopic);";
                    cn.Execute(sql, ce);
                }
                sqliteTransaction.Commit();

            }
        }

        public static void UpdateCompletedCase(PathCase pathCase) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"UPDATE path_case 
                                SET case_number = @CaseNumber, service = @Service, evaluation = @Evaluation
                                WHERE case_number = @CaseNumber;";
                cn.Execute(sql, pathCase);
            }
        }

        public static void InsertNewPartDiagnosisEntry(List<PartDiagnosis> ce, PathCase pathCase) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"INSERT INTO path_case (case_number, service)
                             VALUES (@CaseNumber, @Service);";
                cn.Execute(sql, pathCase);
                sql = @"INSERT INTO part_diagnosis ( case_number, part, date_modified, time_modified,
                                                organ_system, organ, category, diagnosis, diagnosis_detailed)
                            VALUES (@CaseNumber, @Part, @DateModifiedString, @TimeModifiedString,@OrganSystem, @Organ, 
                                    @Category, @Diagnosis, @DiagnosisDetailed);";
                cn.Execute(sql, ce);
            }
        }

        public static List<PartDiagnosis> getListPartDiagnosis(string caseNumber) {
            var sql = @"SELECT case_number AS CaseNumber,
                            part,
                            date_modified AS DateModifiedString, 
                            time_modified AS TimeModifiedString,
                            organ_system AS OrganSystem,
                            organ, category, diagnosis, 
                            diagnosis_detailed AS DiagnosisDetailed
                            FROM part_diagnosis WHERE CaseNumber = @caseNumber;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@caseNumber", caseNumber, System.Data.DbType.String);
                var output = cn.Query<PartDiagnosis>(sql, dp).ToList();
                return output;
            }
        }

        /// <summary>
        /// Retrieves the part diagnosis with all of its versions, and then returns the latest version
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <returns></returns>
        public static List<PartDiagnosis> GetListPartDiagnosisLatestVersion(string caseNumber) {
            List<PartDiagnosis> parts = getListPartDiagnosis(caseNumber);
            DateTime latestDateTimeModified = (from latestTimeModified in parts
                                               orderby latestTimeModified.DateTimeModifiedObject descending
                                               select latestTimeModified.DateTimeModifiedObject).FirstOrDefault();
            List<PartDiagnosis> latestPartEntry = new List<PartDiagnosis>();
            if (latestDateTimeModified != null) {
                latestPartEntry = (from p in parts.ToList<PartDiagnosis>()
                                   where p.DateTimeModifiedObject.Equals(latestDateTimeModified)
                                   select p).ToList();
            }
            return latestPartEntry;
        }

        public static bool EntryExistsPartDiagnosis(string caseNumber) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"select exists(select 1 from part_diagnosis where case_number = @caseNumber LIMIT 1);";
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@caseNumber", caseNumber, System.Data.DbType.String);
                int v = cn.Query<int>(sql, dp).FirstOrDefault();
                bool entryExists = v == 1 ? true : false;
                return entryExists;
            }
               
        }

        public static bool CaseEntryEvaluated(string caseNumber) {
             using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"select count(*) from path_case where case_number = @caseNumber and evaluation not NULL;";
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@caseNumber", caseNumber, System.Data.DbType.String);
                int v = cn.Query<int>(sql, dp).FirstOrDefault();
                bool entryExists = v >= 1 ? true : false;
                return entryExists;
            }
        }

        /// <summary>
        /// Returns true if there is at least 2 authors that have made a case entry on the given case
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <returns></returns>
        public static bool HasMultipleAuthorEntries(string caseNumber) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"select COUNT(DISTINCT author_id) from case_entry where case_number = @CaseNumber";
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@caseNumber", caseNumber, System.Data.DbType.String);
                int v = cn.Query<int>(sql, dp).FirstOrDefault();
                bool multipleAuthors = v > 1 ? true : false;
                return multipleAuthors;

            }
        }

        public static void InsertNewParts(List<PartEntry> parts, PathCase pathCase) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"INSERT INTO path_case (case_number, service)
                             VALUES (@CaseNumber, @Service);";
                cn.Execute(sql, pathCase);
                sql = @"INSERT INTO part_entry (part, procedure,
                            specimen, date_modified, time_modified, case_number, author_id)
                            VALUES (@Part, @Procedure, @Specimen, @DateModifiedString,@TimeModifiedString, @CaseNumber, @AuthorID);";
                cn.Execute(sql, parts);
            }
        }


        public static List<PartEntry> GetListPartEntryLatestVersion(string caseNumber) {
            List<PartEntry> parts = GetListPartEntry(caseNumber);
            DateTime latestDateTimeModified = (from latestTimeModified in parts
                                               orderby latestTimeModified.DateTimeModifiedObject descending
                                               select latestTimeModified.DateTimeModifiedObject).FirstOrDefault();
            List<PartEntry> latestPartEntry = new List<PartEntry>();
            if (latestDateTimeModified != null) {
                latestPartEntry = (from p in parts.ToList<PartEntry>()
                                   where p.DateTimeModifiedObject.Equals(latestDateTimeModified)
                                   select p).ToList();
            }
            return latestPartEntry;
        }


        public static List<string> GetListPartEntryPastDays(DateTime startDate) {
            var sql = @"SELECT DISTINCT case_number AS CaseNumber, specimen, procedure, date_modified AS DateModifiedString
                            FROM part_entry WHERE date_modified >= @startDate";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@startDate", startDate.ToString("yyyy-MM-dd"), System.Data.DbType.String);
                var output = cn.Query<string>(sql, dp).ToList();
                return output;
            }
        }

        public static List<CaseEntry> GetListCaseEntryPastDays(DateTime startDate) {
            var sql = @"SELECT DISTINCT case_number AS CaseNumber, material, date_modified AS DateModifiedString
                            FROM case_entry WHERE date_modified >= @startDate";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@startDate", startDate.ToString("yyyy-MM-dd"), System.Data.DbType.String);
                var output = cn.Query<CaseEntry>(sql, dp).ToList();
                return output;
            }
        }

        public static List<CaseEntry> GetListAllCaseEntries() {
            var sql = @"SELECT * FROM (SELECT case_number AS CaseNumber,
                        author_id AS AuthorID,
	                    date_modified AS DateModifiedString,
	                    time_modified AS TimeModifiedString,
	                    tumor_synoptic AS TumorSynoptic,
	                    comment,
	                    result,
	                    material,
	                    history,
	                    interpretation,
	                    gross,
	                    microscopic FROM case_entry 
                                        ORDER BY DateModifiedString ASC,
                                        TimeModifiedString ASC)
                                    GROUP BY CaseNumber
                                    ORDER BY DateModifiedString DESC,
                                    TimeModifiedString DESC;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                var output = cn.Query<CaseEntry>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        /// <summary>
        /// Returns a list of all case entries entered after the supplied start_date.
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static List<CaseEntry> FilterCaseEntryDateModified(DateTime startDate) {
            var sql = @"SELECT * FROM (SELECT case_number AS CaseNumber,
                        author_id AS AuthorID,
	                    date_modified AS DateModifiedString,
	                    time_modified AS TimeModifiedString,
	                    tumor_synoptic AS TumorSynoptic,
	                    comment,
	                    result,
	                    material,
	                    history,
	                    interpretation,
	                    gross,
	                    microscopic FROM case_entry 
                        WHERE date_modified >= @startDate 
                                        ORDER BY DateModifiedString ASC,
                                        TimeModifiedString ASC)
                                  GROUP BY CaseNumber
                                  ORDER BY DateModifiedString DESC,
                                  TimeModifiedString DESC ";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                // SqlLite requires date to be in year-month-day format for sorting purposes
                dp.Add("@startDate", startDate.ToString("yyyy-MM-dd"), System.Data.DbType.String);
                var output = cn.Query<CaseEntry>(sql, dp).ToList();
                return output;
            }
        }
        
      

        public static List<PartEntry> GetListPartEntry(string caseNumber) {
            var sql = @"SELECT author_id AS AuthorID, 
                            part, procedure,
                            specimen, 
                            date_modified AS DateModifiedString, time_modified AS TimeModifiedString,
                            case_number AS CaseNumber 
                            FROM part_entry WHERE CaseNumber = @caseNumber;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@caseNumber", caseNumber, System.Data.DbType.String);
                var output = cn.Query<PartEntry>(sql, dp).ToList();
                return output;
            }
        }

        public static List<CaseEntry> GetCaseEntryFilterAuthorID(string authorID) {
            var sql = @"SELECT id,
	                    author_id AS AuthorID,
	                    case_number AS CaseNumber,
	                    date_modified AS DateModifiedString,
	                    time_modified AS TimeModifiedString,
	                    tumor_synoptic AS TumorSynoptic,
	                    comment,
	                    result,
	                    material,
	                    history,
	                    interpretation,
	                    gross,
	                    microscopic FROM case_entry WHERE author_id = @authorID;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@authorID", authorID, System.Data.DbType.String);
                var output = cn.Query<CaseEntry>(sql, dp).ToList();
                return output;
            }
        }

        public static List<CaseEntry> GetListCaseEntry(string caseNumber) {
            var sql = @"SELECT id,
	                    author_id AS AuthorID,
	                    case_number AS CaseNumber,
	                    date_modified AS DateModifiedString,
	                    time_modified AS TimeModifiedString,
	                    tumor_synoptic AS TumorSynoptic,
	                    comment,
	                    result,
	                    material,
	                    history,
	                    interpretation,
	                    gross,
	                    microscopic FROM case_entry WHERE case_number = @caseNumber ORDER By DateModifiedString ASC, TimeModifiedString ASC;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@caseNumber", caseNumber, System.Data.DbType.String);
                var output = cn.Query<CaseEntry>(sql, dp).ToList();
                return output;
            }
        }

        public static PathCase GetPathCase(string caseNumber) {
            var sql = @"SELECT 
	                    case_number AS CaseNumber,
	                   evaluation, service FROM path_case WHERE case_number = @caseNumber;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@caseNumber", caseNumber, System.Data.DbType.String);
                var output = cn.Query<PathCase>(sql, dp).ToList().FirstOrDefault();
                return output;
            }
        }

        public static CaseEntry GetCaseEntryLatestVersion(string caseNumber) {
            List<CaseEntry> cases = GetListCaseEntry(caseNumber);
            DateTime latestDateTimeModified = (from latestTimeModified in cases
                                               orderby latestTimeModified.DateTimeModifiedObject descending
                                               select latestTimeModified.DateTimeModifiedObject).FirstOrDefault();
            CaseEntry latestCaseEntry = new CaseEntry();
            if (latestDateTimeModified != null) {
                latestCaseEntry = (from p in cases.ToList<CaseEntry>()
                                   where p.DateTimeModifiedObject.Equals(latestDateTimeModified)
                                   select p).FirstOrDefault();
            }
            return latestCaseEntry;
        }

        public static List<DateTime> GetCaseEntryListDateTimeModified(string caseNumber) {
            var sql = @"select author_id, date_modified, time_modified from case_entry 
                                                    WHERE case_number = @caseNumber
                                        ORDER BY date_modified ASC,
                                        time_modified ASC;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@caseNumber", caseNumber, System.Data.DbType.String);
                var output = cn.Query(sql, dp).ToList();
                var listDateTimeModified = new List<DateTime>();
                for (int i =0; i < output.Count -1; i++) { // we want to skip the last entry, which is the attending's entry
                    var data = (IDictionary<string, object>)output[i];
                     listDateTimeModified.Add(DateTime.Parse(data["date_modified"].ToString() + " " + data["time_modified"].ToString()));
                }
                return listDateTimeModified;
            }
        }

        public static List<Staff> GetListAuthor() {
            var sql = @"SELECT author_id as AuthorID,
                               last_first_name AS LastFirstName,
                               role AS Role FROM staff;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<Staff>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }
              
        public static List<string> GetUniqueProcedure() {
            var sql = @"SELECT procedure FROM procedure;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        public static List<string> GetUniqueSpecimen() {
            var sql = @"SELECT specimen FROM specimen;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        public static List<string> GetUniqueDiagnosticCategory() {
            var sql = @"SELECT category FROM diagnosis_category;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        public static List<string> GetUniqueDiagnosis() {
            var sql = @"SELECT diagnosis FROM diagnosis;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        public static List<string> GetUniqueEvaluations() {
            var sql = @"SELECT evaluation FROM evaluation;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        public static List<string> GetUniqueService() {
            var sql = @"SELECT service FROM service;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }


        public static List<string> GetListOrgan() {
            var sql = @"SELECT organ FROM organ;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        public static List<string> GetListOrganSystem() {
            var sql = @"SELECT organ_system FROM organ_system;";
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var output = cn.Query<string>(sql, new DynamicParameters()).ToList();
                return output;
            }
        }

        public static List<CaseEntry> FilterCaseEntryInterpretation(string strFilterInterpretation) {
            return FilterCaseEntry(strFilterInterpretation, "fts5_case_entry_interpretation");
        }

        public static List<CaseEntry> FilterCaseEntryResult(string strFilterResult) {
            return FilterCaseEntry(strFilterResult, "fts5_case_entry_result");
        }

        public static List<CaseEntry> FilterCaseEntryComment(string strFilterComment) {
            return FilterCaseEntry(strFilterComment, "fts5_case_entry_comment");
        }

        public static List<CaseEntry> FilterCaseEntryTumorSynoptic(string strFilterTumorSynoptic) {
            return FilterCaseEntry(strFilterTumorSynoptic, "fts5_case_entry_tumor_synoptic");
        }
        
        private static List<CaseEntry> FilterCaseEntry(string strFilter, string fts5TableName) {
            var sql = @"SELECT id,
	                    case_entry.author_id AS AuthorID,
	                    case_entry.case_number AS CaseNumber,
	                    case_entry.date_modified AS DateModifiedString,
	                    case_entry.time_modified AS TimeModifiedString,
	                    case_entry.tumor_synoptic AS TumorSynoptic,
	                    case_entry.comment,
	                    case_entry.result,
	                    case_entry.material,
	                    case_entry.history,
	                    case_entry.interpretation,
	                    case_entry.gross,
	                    case_entry.microscopic FROM " + fts5TableName.Trim() + @"
                        INNER JOIN case_entry ON case_entry.case_number = "+ fts5TableName.Trim() +@".case_number
                        WHERE "+ fts5TableName.Trim() + @" MATCH @strFilter;";

            using (var cn = new SQLiteConnection(DbConnectionString)) {
                cn.Open();
                cn.EnableExtensions(true);
                cn.LoadExtension("SQLite.Interop.dll", "sqlite3_fts5_init"); // required extension in order to do FTS5 search
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@strFilter", strFilter, System.Data.DbType.String);
                var output = cn.Query<CaseEntry>(sql, dp).ToList();
                cn.Close();
                return output;
            }
        }

        public static string DbConnectionString {
            get {
               
                return @"Data Source = "+ DBPath; 
            } 
        }

        public static string DBPath {
            get {
                var  dbPath = Properties.Settings.Default.DatabasePath;
                return dbPath;
            }
            set {
                Properties.Settings.Default.DatabasePath = value;
                Properties.Settings.Default.Save();
            }

        }

        /// <summary>
        /// Creates the Casely database
        /// </summary>
        public static void CreateDatabase() {
            if (!(File.Exists(DBPath))) {
                if (Directory.Exists(Path.GetDirectoryName(DBPath))) {
                    var f = File.Create(DBPath);
                    f.Close();
                } else {
                    System.Windows.Forms.MessageBox.Show($"Could not create database at {SqliteDataAcces.DBPath}. Does the folder exist?","",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }
              
            }
            
            using (var cn = new SQLiteConnection(DbConnectionString)) {
               cn.Open();
               cn.EnableExtensions(true);
               cn.LoadExtension("SQLite.Interop.dll", "sqlite3_fts5_init");
               cn.Execute(DBCreationString.sqlCreateDBString);
               cn.Close();
            }
        }

        /// <summary>
        /// Inserts a new pathology case into the database.
        /// Case number must be unique. If not unique, insertion will be ignored by sqlite database.
        /// </summary>
        /// <param name="pathCase"></param>
        public static void InsertNewPathCase(PathCase pathCase) {
            using (var cn = new SQLiteConnection(DbConnectionString)) {
                var sql = @"INSERT INTO path_case (case_number, service) VALUES (@CaseNumber, @Service);";
                DynamicParameters dp = new DynamicParameters();
                dp.Add("@CaseNumber", pathCase.CaseNumber, System.Data.DbType.String);
                dp.Add("@Service", pathCase.Service, System.Data.DbType.String);
                var result = cn.Execute(sql, dp);
            }
        }

    }

}
