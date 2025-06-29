using Excel = Microsoft.Office.Interop.Excel;
using DBT_API.Entities;

namespace DBT_API.Util
{
    public static class ScheduleHelper
    {
        public static List<Node> ReadSchedule(string filePath, string domain, string filePathLinks, List<List<string>> IDs)
        {
            List<Node> nodes = new();

            Excel.Application xlApplication = new()
            {
                Visible = false
            };
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            Excel.Application xlApplication2 = new()
            {
                Visible = false
            };
            Excel.Workbook xlWorkBook2;
            Excel.Worksheet xlWorkSheet2;
            Excel.Range range2;

            try
            {
                xlWorkBook = xlApplication.Workbooks.Open(filePath, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                xlWorkBook2 = xlApplication2.Workbooks.Open(filePathLinks, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                xlWorkSheet2 = (Excel.Worksheet)xlWorkBook2.Worksheets.get_Item(1);

                range = xlWorkSheet.UsedRange;
                int rowCount = range.Rows.Count;
                int columnCount = range.Columns.Count;

                range2 = xlWorkSheet2.UsedRange;
                int rowCount2 = range2.Rows.Count;
                int columnCount2 = range2.Rows.Count;

                ConstructionSchedule constructionSchedule = new()
                {
                    Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#ConstructionSchedule" },
                    Domain = domain,
                    IRI = domain + "initialSchedule",
                    _name = "Initial construction schedule",
                    _baselinePlanFrom = new DateTime(2022, 7, 1)
                    //_baselinePlanTill = new DateTime()
                };
                List<Edge> scheduleEdges = new();

                for (int i = 2; i <= rowCount; i++) // rowCount
                {
                    int workpackageID = (int)(xlWorkSheet.Cells[i, 1] as Excel.Range).Value;
                    string workpackageName = (xlWorkSheet.Cells[i, 2] as Excel.Range).Text.ToString();
                    DateTime startTime = (xlWorkSheet.Cells[i, 4] as Excel.Range).Value;
                    DateTime endTime = (xlWorkSheet.Cells[i, 5] as Excel.Range).Value;
                    string preconditions = (xlWorkSheet.Cells[i, 6] as Excel.Range).Text.ToString();

                    // read links (schedule -> IFC)
                    string processID = (xlWorkSheet2.Cells[i, 1] as Excel.Range).Text.ToString();
                    string ifcguids = (xlWorkSheet2.Cells[i, 2] as Excel.Range).Text.ToString();

                    // split preconditions string into separate process IDs
                    List<int> preconditionIDs = new();
                    string[] splitPreconditions = preconditions.Split(',');
                    if (splitPreconditions[0] != "")
                    {
                        foreach (var precon in splitPreconditions)
                        {
                            preconditionIDs.Add(Int32.Parse(precon));
                        }
                    }

                    // split ifcguid string into separate IDs
                    List<string> ifcLinkIDs = new();
                    string[] splitIDs = ifcguids.Split(',');
                    if (splitIDs[0] != "")
                    {
                        foreach (var linkID in splitIDs)
                        {
                            ifcLinkIDs.Add(linkID.ToString()); // " " at the beginning of some (all but the first one)
                        }
                    }

                    List<Edge> wpEdges = new();

                    AsPlannedProcess workPackage = new()
                    {
                        Domain = domain,
                        IRI = domain + "workpackage" + workpackageID.ToString(),
                        _id = workpackageID,
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#AsPlannedProcess" },
                        _name = workpackageName,
                    };

                    Edge scheduleEdge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasProcess", workPackage.IRI);
                    scheduleEdges.Add(scheduleEdge);

                    if (preconditionIDs.Count != 0)
                    {
                        for (int j = 0; j < preconditionIDs.Count; j++)
                        {
                            ProcessPrecondition pp = new()
                            {
                                Domain = domain,
                                IRI = domain + "precondition" + workpackageID.ToString() + "_" + j.ToString(),
                                //_id
                                Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#ProcessPrecondition" },
                                _fulfilled = false,
                                //_hasSequenceType = SequenceType.StartEnd
                            };

                            string ppTargetIri = domain + "workpackage" + preconditionIDs[j].ToString();
                            Edge ppEdge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#requiresProcess", ppTargetIri);
                            pp.Relations = new List<Edge> { ppEdge };

                            AddEdge(pp, "https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasSequenceType", SequenceType.StartEnd);

                            string wpTargetIri = domain + "precondition" + workpackageID.ToString() + "_" + j.ToString();
                            Edge wpEdge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasPrecondition", wpTargetIri);
                            wpEdges.Add(wpEdge);

                            nodes.Add(pp);
                        }
                    }

                    // add activities for cast in place concreting
                    // place rebar
                    AsPlannedProcess act1 = new()
                    {
                        Domain = domain,
                        IRI = domain + "activity" + workpackageID.ToString() + "_1",
                        //_id = 0,
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#AsPlannedProcess" },
                        _name = "Place rebar for " + workpackageName,
                        _startTime = startTime,
                        _endTime = endTime,
                        _classificationSystem = "Omniclass",
                        _classificationCode = "22-03 20 00"
                    };
                    List<Edge> act1Edges = new();

                    int counter = 1;
                    foreach (string link in ifcLinkIDs)
                    {
                        AsPlannedProcess taski = new()
                        {
                            Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#AsPlannedProcess" },
                            Domain = domain,
                            IRI = domain + "task" + workpackageID.ToString() + "_1" + "_" + counter.ToString(),
                            _contractor = "company name",
                            _name = "Place rebar for " + workpackageName + " for element " + link,
                            _startTime = startTime,
                            _endTime = endTime,
                            _classificationSystem = "Omniclass",
                            _classificationCode = "22-03 20 00"
                        };
                        string elementTargetIri = " ";
                        for (int j = 0; j < IDs.ElementAt(0).Count; j++)
                        {
                            if (IDs.ElementAt(0).ElementAt(j) == link)
                            {
                                elementTargetIri = IDs.ElementAt(1).ElementAt(j);
                                break;
                            }
                        }
                        //string elementTargetIri = domain + "ifc-" + link;
                        Edge taskEdge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasTarget", elementTargetIri);
                        taski.Relations = new List<Edge> { taskEdge };
                        nodes.Add(taski);

                        string taskTargetIri = domain + "activity" + workpackageID.ToString() + "_1";
                        Edge actEdge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasChildProcess", taski.IRI);
                        act1Edges.Add(actEdge);

                        counter++;
                    }

                    // add process decomposition
                    ProcessDecomposition decompAct1 = new()
                    {
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#ProcessDecomposition" },
                        Domain = domain,
                        IRI = domain + "decomposition_act_" + workpackageID.ToString() + "_1",
                        _name = "Process decomposition for activity " + workpackageID.ToString() + "_1",
                        _decompositionLevel = 2,
                    };

                    string act1DecompositionTarget = domain + "decomposition_act_" + workpackageID.ToString() + "_1";
                    Edge act1Decomp = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#isDecomposedInto", act1DecompositionTarget);
                    act1Edges.Add(act1Decomp);

                    DecompositionCriterium critAct1 = new()
                    {
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#DecompositionCriterium" },
                        Domain = domain,
                        IRI = domain + "decomposition_crit_act_" + workpackageID.ToString() + "_1",
                        _name = "Element",
                    };

                    string decompAct1CritTarget = domain + "decomposition_crit_wp_" + workpackageID.ToString() + "_1";
                    Edge decompAct1Crit = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasDecompositionCriterium", decompAct1CritTarget);
                    decompAct1.Relations = new List<Edge> { decompAct1Crit };

                    nodes.Add(decompAct1);
                    nodes.Add(critAct1);

                    act1.Relations = act1Edges;
                    nodes.Add(act1);

                    string wpTargetIri1 = domain + "activity" + workpackageID.ToString() + "_1";
                    Edge wpEdge1 = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasChildProcess", wpTargetIri1);
                    wpEdges.Add(wpEdge1);

                    // place formwork
                    AsPlannedProcess act2 = new()
                    {
                        Domain = domain,
                        IRI = domain + "activity" + workpackageID.ToString() + "_2",
                        //_id = 0,
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#AsPlannedProcess" },
                        _name = "Place formwork for " + workpackageName,
                        _startTime = startTime,
                        _endTime = endTime,
                        _classificationSystem = "Uniclass",
                        _classificationCode = "Ac_10_40_30",
                    };
                    List<Edge> act2Edges = new();

                    ProcessPrecondition pp1 = new()
                    {
                        Domain = domain,
                        IRI = domain + "precondition" + workpackageID.ToString() + "_2_1",
                        //_id
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#ProcessPrecondition" },
                        _fulfilled = false,
                        //_hasSequenceType = SequenceType.StartEnd
                    };

                    string pp1TargetIri = domain + "activity" + workpackageID.ToString() + "_1";
                    Edge pp1Edge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#requiresProcess", pp1TargetIri);
                    pp1.Relations = new List<Edge> { pp1Edge };

                    AddEdge(pp1, "https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasSequenceType", SequenceType.StartEnd);

                    string act2TargetIri = domain + "precondition" + workpackageID.ToString() + "_2_1";
                    Edge act2Edge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasPrecondition", act2TargetIri);
                    act2Edges.Add(act2Edge);

                    nodes.Add(pp1);

                    counter = 1;
                    foreach (string link in ifcLinkIDs)
                    {
                        AsPlannedProcess taski = new()
                        {
                            Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#AsPlannedProcess" },
                            Domain = domain,
                            IRI = domain + "task" + workpackageID.ToString() + "_2" + "_" + counter.ToString(),
                            _contractor = "company name",
                            _name = "Place formwork for " + workpackageName + " for element " + link,
                            _startTime = startTime,
                            _endTime = endTime,
                            _classificationSystem = "Uniclass",
                            _classificationCode = "Ac_10_40_30",
                        };
                        string elementTargetIri = " ";
                        for (int j = 0; j < IDs.ElementAt(0).Count; j++)
                        {
                            if (IDs.ElementAt(0).ElementAt(j) == link)
                            {
                                elementTargetIri = IDs.ElementAt(1).ElementAt(j);
                                break;
                            }
                        }
                        //string elementTargetIri = domain + "ifc-" + link;
                        Edge taskEdge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasTarget", elementTargetIri);
                        taski.Relations = new List<Edge> { taskEdge };
                        nodes.Add(taski);

                        string taskTargetIri = domain + "activity" + workpackageID.ToString() + "_2";
                        Edge actEdge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasChildProcess", taski.IRI);
                        act2Edges.Add(actEdge);

                        counter++;
                    }

                    // add process decomposition
                    ProcessDecomposition decompAct2 = new()
                    {
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#ProcessDecomposition" },
                        Domain = domain,
                        IRI = domain + "decomposition_act_" + workpackageID.ToString() + "_2",
                        _name = "Process decomposition for activity " + workpackageID.ToString() + "_2",
                        _decompositionLevel = 2,
                    };

                    string act2DecompositionTarget = domain + "decomposition_act_" + workpackageID.ToString() + "_2";
                    Edge act2Decomp = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#isDecomposedInto", act2DecompositionTarget);
                    act2Edges.Add(act2Decomp);

                    DecompositionCriterium critAct2 = new()
                    {
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#DecompositionCriterium" },
                        Domain = domain,
                        IRI = domain + "decomposition_crit_act_" + workpackageID.ToString() + "_2",
                        _name = "Element",
                    };

                    string decompAct2CritTarget = domain + "decomposition_crit_wp_" + workpackageID.ToString() + "_2";
                    Edge decompAct2Crit = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasDecompositionCriterium", decompAct2CritTarget);
                    decompAct2.Relations = new List<Edge> { decompAct2Crit };

                    nodes.Add(decompAct2);
                    nodes.Add(critAct2);

                    act2.Relations = act2Edges;
                    nodes.Add(act2);

                    string wpTargetIri2 = domain + "activity" + workpackageID.ToString() + "_2";
                    Edge wpEdge2 = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasChildProcess", wpTargetIri2);
                    wpEdges.Add(wpEdge2);

                    // pour concrete
                    AsPlannedProcess act3 = new()
                    {
                        Domain = domain,
                        IRI = domain + "activity" + workpackageID.ToString() + "_3",
                        //_id = 0,
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#AsPlannedProcess" },
                        _name = "Pouring concrete for " + workpackageName,
                        _startTime = startTime,
                        _endTime = endTime,
                        _classificationSystem = "Omniclass",
                        _classificationCode = "32-57 61 15",
                    };
                    List<Edge> act3Edges = new();

                    ProcessPrecondition pp2 = new()
                    {
                        Domain = domain,
                        IRI = domain + "precondition" + workpackageID.ToString() + "_3_1",
                        //_id
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#ProcessPrecondition" },
                        _fulfilled = false,
                        //_hasSequenceType = SequenceType.StartEnd
                    };

                    string pp2TargetIri = domain + "activity" + workpackageID.ToString() + "_2";
                    Edge pp2Edge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#requiresProcess", pp2TargetIri);
                    pp2.Relations = new List<Edge> { pp2Edge };

                    AddEdge(pp2, "https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasSequenceType", SequenceType.StartEnd);

                    string act3TargetIri = domain + "precondition" + workpackageID.ToString() + "_3_1";
                    Edge act3Edge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasPrecondition", act3TargetIri);
                    act3Edges.Add(act3Edge);

                    nodes.Add(pp2);

                    counter = 1;
                    foreach (string link in ifcLinkIDs)
                    {
                        AsPlannedProcess taski = new()
                        {
                            Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#AsPlannedProcess" },
                            Domain = domain,
                            IRI = domain + "task" + workpackageID.ToString() + "_3" + "_" + counter.ToString(),
                            _contractor = "company name",
                            _name = "Pouring concrete for " + workpackageName + " for element " + link,
                            _startTime = startTime,
                            _endTime = endTime,
                            _classificationSystem = "Omniclass",
                            _classificationCode = "32-57 61 15",
                        };
                        string elementTargetIri = " ";
                        for (int j = 0; j < IDs.ElementAt(0).Count; j++)
                        {
                            if (IDs.ElementAt(0).ElementAt(j) == link)
                            {
                                elementTargetIri = IDs.ElementAt(1).ElementAt(j);
                                break;
                            }
                        }
                        //string elementTargetIri = domain + "ifc-" + link;
                        Edge taskEdge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasTarget", elementTargetIri);
                        taski.Relations = new List<Edge> { taskEdge };
                        nodes.Add(taski);

                        string taskTargetIri = domain + "activity" + workpackageID.ToString() + "_3";
                        Edge actEdge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasChildProcess", taski.IRI);
                        act3Edges.Add(actEdge);

                        counter++;
                    }

                    // add process decomposition
                    ProcessDecomposition decompAct3 = new()
                    {
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#ProcessDecomposition" },
                        Domain = domain,
                        IRI = domain + "decomposition_act_" + workpackageID.ToString() + "_3",
                        _name = "Process decomposition for activity " + workpackageID.ToString() + "_3",
                        _decompositionLevel = 2,
                    };

                    string act3DecompositionTarget = domain + "decomposition_act_" + workpackageID.ToString() + "_3";
                    Edge act3Decomp = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#isDecomposedInto", act3DecompositionTarget);
                    act3Edges.Add(act3Decomp);

                    DecompositionCriterium critAct3 = new()
                    {
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#DecompositionCriterium" },
                        Domain = domain,
                        IRI = domain + "decomposition_crit_act_" + workpackageID.ToString() + "_3",
                        _name = "Element",
                    };

                    string decompAct3CritTarget = domain + "decomposition_crit_wp_" + workpackageID.ToString() + "_3";
                    Edge decompAct3Crit = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasDecompositionCriterium", decompAct3CritTarget);
                    decompAct3.Relations = new List<Edge> { decompAct3Crit };

                    nodes.Add(decompAct3);
                    nodes.Add(critAct3);

                    act3.Relations = act3Edges;
                    nodes.Add(act3);

                    string wpTargetIri3 = domain + "activity" + workpackageID.ToString() + "_3";
                    Edge wpEdge3 = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasChildProcess", wpTargetIri3);
                    wpEdges.Add(wpEdge3);

                    // remove formwork
                    AsPlannedProcess act4 = new()
                    {
                        Domain = domain,
                        IRI = domain + "activity" + workpackageID.ToString() + "_4",
                        //_id = 0,
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#AsPlannedProcess" },
                        _name = "Remove formwork from " + workpackageName,
                        _startTime = startTime,
                        _endTime = endTime,
                        _classificationSystem = "Uniclass",
                        _classificationCode = "Ac_10_40_30",
                    };
                    List<Edge> act4Edges = new();

                    ProcessPrecondition pp3 = new()
                    {
                        Domain = domain,
                        IRI = domain + "precondition" + workpackageID.ToString() + "_4_1",
                        //_id
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#ProcessPrecondition" },
                        _fulfilled = false,
                        //_hasSequenceType = SequenceType.StartEnd
                    };

                    string pp3TargetIri = domain + "activity" + workpackageID.ToString() + "_3";
                    Edge pp3Edge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#requiresProcess", pp3TargetIri);
                    pp3.Relations = new List<Edge> { pp3Edge };

                    AddEdge(pp3, "https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasSequenceType", SequenceType.StartEnd);

                    string act4TargetIri = domain + "precondition" + workpackageID.ToString() + "_4_1";
                    Edge act4Edge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasPrecondition", act4TargetIri);
                    act4Edges.Add(act4Edge);

                    nodes.Add(pp3);

                    counter = 1;
                    foreach (string link in ifcLinkIDs)
                    {
                        AsPlannedProcess taski = new()
                        {
                            Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#AsPlannedProcess" },
                            Domain = domain,
                            IRI = domain + "task" + workpackageID.ToString() + "_4" + "_" + counter.ToString(),
                            _contractor = "company name",
                            _name = "Removing formwork from " + workpackageName + " for element " + link,
                            _startTime = startTime,
                            _endTime = endTime,
                            _classificationSystem = "Uniclass",
                            _classificationCode = "Ac_10_40_30",
                        };
                        string elementTargetIri = " ";
                        for (int j = 0; j < IDs.ElementAt(0).Count; j++)
                        {
                            if (IDs.ElementAt(0).ElementAt(j) == link)
                            {
                                elementTargetIri = IDs.ElementAt(1).ElementAt(j);
                                break;
                            }
                        }
                        //string elementTargetIri = domain + "ifc-" + link;
                        Edge taskEdge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasTarget", elementTargetIri);
                        taski.Relations = new List<Edge> { taskEdge };
                        nodes.Add(taski);

                        string taskTargetIri = domain + "activity" + workpackageID.ToString() + "_4";
                        Edge actEdge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasChildProcess", taski.IRI);
                        act4Edges.Add(actEdge);

                        counter++;
                    }

                    // add process decomposition
                    ProcessDecomposition decompAct4 = new()
                    {
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#ProcessDecomposition" },
                        Domain = domain,
                        IRI = domain + "decomposition_act_" + workpackageID.ToString() + "_4",
                        _name = "Process decomposition for activity " + workpackageID.ToString() + "_4",
                        _decompositionLevel = 2,
                    };

                    string act4DecompositionTarget = domain + "decomposition_act_" + workpackageID.ToString() + "_4";
                    Edge act4Decomp = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#isDecomposedInto", act4DecompositionTarget);
                    act4Edges.Add(act4Decomp);

                    DecompositionCriterium critAct4 = new()
                    {
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#DecompositionCriterium" },
                        Domain = domain,
                        IRI = domain + "decomposition_crit_act_" + workpackageID.ToString() + "_4",
                        _name = "Element",
                    };

                    string decompAct4CritTarget = domain + "decomposition_crit_wp_" + workpackageID.ToString() + "_4";
                    Edge decompAct4Crit = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasDecompositionCriterium", decompAct4CritTarget);
                    decompAct4.Relations = new List<Edge> { decompAct4Crit };

                    nodes.Add(decompAct4);
                    nodes.Add(critAct4);

                    act4.Relations = act4Edges;
                    nodes.Add(act4);

                    string wpTargetIri4 = domain + "activity" + workpackageID.ToString() + "_4";
                    Edge wpEdge4 = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasChildProcess", wpTargetIri4);
                    wpEdges.Add(wpEdge4);

                    // add process decomposition
                    ProcessDecomposition decomp = new()
                    {
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#ProcessDecomposition" },
                        Domain = domain,
                        IRI = domain + "decomposition_wp_" + workpackageID.ToString(),
                        _name = "Process decomposition for work package " + workpackageID.ToString(),
                        _decompositionLevel = 1,
                    };

                    string wpDecompositionTarget = domain + "decomposition_wp_" + workpackageID.ToString();
                    Edge wpDecomp = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#isDecomposedInto", wpDecompositionTarget);
                    wpEdges.Add(wpDecomp);

                    DecompositionCriterium crit = new()
                    {
                        Classes = new List<string> { "https://dtc-ontology.cms.ed.tum.de/ontology/v2#DecompositionCriterium" },
                        Domain = domain,
                        IRI = domain + "decomposition_crit_wp_" + workpackageID.ToString(),
                        _name = "Construction Step",
                    };

                    string decompCritTarget = domain + "decomposition_crit_wp_" + workpackageID.ToString();
                    Edge decompCrit = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasDecompositionCriterium", decompCritTarget);
                    decomp.Relations = new List<Edge> { decompCrit };

                    nodes.Add(decomp);
                    nodes.Add(crit);

                    // add workpackage to list of avatars since now all edges are created
                    workPackage.Relations = wpEdges;
                    nodes.Add(workPackage);
                }

                constructionSchedule.Relations = scheduleEdges;
                nodes.Add(constructionSchedule);

                xlWorkBook.Close(false, Type.Missing, Type.Missing);
                xlApplication.Quit();

                return nodes;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occured when trying to read file", ex);
            }
        }
        public static Edge CreateEdge(string name, string iri) 
        {
            Edge edge = new()
            {
                Name = name,
                ObjectIRI = iri
            };
            return edge;
        }

        public static void AddEdge(List<Edge> edges, string name, string iri)
        {
            Edge edge = new()
            {
                Name = name,
                ObjectIRI = iri
            };
            edges.Add(edge);
        }

        public static void AddEdge(Node node, string name, string iri)
        {
            Edge edge = new()
            {
                Name = name,
                ObjectIRI = iri
            };
            node.Relations.Add(edge);
        }
    }
}
