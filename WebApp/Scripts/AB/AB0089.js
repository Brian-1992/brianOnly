Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {

    var T1Rec = 0;
    var T1LastRec = null;
    var T2Rec = 0;
    var T2LastRec = null;
    var T3Rec = 0;
    var T3LastRec = null;
    
    var T1GetExcel = '/api/AB0089/Excel_T1';
    var T2GetExcel = '/api/AB0089/Excel_T2';
    var T3GetExcel = '/api/AB0089/Excel_T3';
    
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 查詢欄位

    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    function get6monthday() {
        var rtnDay = addMonths(new Date(), -6);
        return rtnDay
    }
    function get1monthday() {
        var rtnDay = addMonths(new Date(), -1);
        return rtnDay
    }
    function addMonths(date, months) {
        date.setMonth(date.getMonth() + months);
        return date;
    }
    var mLabelWidth = 50;
    var mWidth = 210;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '異動日期區間',
                    labelAlign: 'right',
                    name: 'P0',
                    id: 'P0',
                    vtype: 'dateRange',
                    dateRange: { end: 'P2' },
                    padding: '0 4 0 4',
                    editable: false,
                    labelWidth: 90,
                    width: 230, 
                    value: get1monthday()
                }, {
                    xtype: 'timefield',
                    format: 'H:i',
                    id: 'P1',
                    name: 'P1',
                    width: 80,
                    maxLength: 5,
                    margin: '0 0 0 1',
                    editable: false,
                    //regexText: '正確格式 HH:MM',
                    //regex: /^(([0-1][0-9])|(2[0-3])):[0-5][0-9]$/, //00:00~23:59
                    increment: 1
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelAlign: 'right',
                    labelWidth: 10,
                    name: 'P2',
                    id: 'P2',
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'P0' },
                    padding: '0 4 0 4',
                    editable: false,
                    width: 150, 
                    value: getToday()
                }, {
                    xtype: 'timefield',
                    format: 'H:i',
                    id: 'P3',
                    name: 'P3',
                    width: 80,
                    maxLength: 5,
                    margin: '0 0 0 1',
                    editable: false,
                    //regexText: '正確格式 HH:MM',
                    //regex: /^(([0-1][0-9])|(2[0-3])):[0-5][0-9]$/, //00:00~23:59
                    increment: 1
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        msglabel("");
                        var activeTab = TATabs.getActiveTab();
                        //var activeTabIndex = TATabs.items.findIndex('id', activeTab.id);
                        if (activeTab.itemId == 't1Grid') {
                            T1Load();
                        }
                        if (activeTab.itemId == 't2Grid') {
                            T2Load();
                        }
                        if (activeTab.itemId == 't3Grid') {
                            T3Load();
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });


    var T1Store = Ext.create('WEBAPP.store.AB0089_1', { // 定義於/Scripts/app/store/AB0089_1.js
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').rawValue,
                    p3: T1Query.getForm().findField('P3').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export1', text: '匯出', handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: 'BASORDM(醫囑線上使用).xls' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').rawValue });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').rawValue });
                    PostForm(T1GetExcel, p);
                    msglabel('匯出完成');
                }
            }
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            /*dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {*/
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        },
        {
            text: "院內代碼",
            dataIndex: 'OrderCode',
            width: 80
        }, {
            text: "記錄處理日期/時間",
            dataIndex: 'ProcDateTime',
            width: 200
        }, {
            text: "記錄處理人員",
            dataIndex: 'ProcOpID',
            width: 100
        }, {
            text: "英文名稱",
            dataIndex: 'OrderEngName',
            width: 100
        }, {
            text: "中文名稱",
            dataIndex: 'OrderChinName',
            width: 100
        }, {
            text: "成份名稱",
            dataIndex: 'ScientificName',
            width: 100
        }, {
            text: "醫囑單位",
            dataIndex: 'OrderUnit',
            width: 100
        }, {
            text: "中文單位",
            dataIndex: 'OrderChinUnit',
            width: 100
        }, {
            text: "申報(計價)單位",
            dataIndex: 'AttachUnit',
            width: 100
        }, {
            text: "扣庫單位",
            dataIndex: 'StockUnit',
            width: 80
        }, {
            text: "藥品藥材代碼",
            dataIndex: 'SkOrderCode',
            width: 100
        }, {
            text: "UD服務",
            dataIndex: 'UDServiceFlag',
            width: 100
        }, {
            text: "服用藥別",
            dataIndex: 'TakeKind',
            width: 100
        }, {
            text: "門診倍數核發",
            dataIndex: 'LimitedQtyO',
            width: 100
        }, {
            text: "住院倍數核發",
            dataIndex: 'LimitedQtyI',
            width: 100
        }, {
            text: "買斷藥",
            dataIndex: 'LimitedQtyI',
            width: 100
        }, {
            text: "系統啟用日期",
            dataIndex: 'OpenDate',
            width: 100
        }, {
            text: "公藥否(Y/N)",
            dataIndex: 'PublicDrugFlag',
            width: 100
        //}, {
        //    text: "HP專用",
        //    dataIndex: '????',
        //    width: 100
        }, {
            text: "開始日期",
            dataIndex: 'StartDate',
            width: 100
        }, {
            text: "院內名稱",
            dataIndex: 'OrderHospName',
            width: 100
        }, {
            text: "簡稱(hp專用)",
            dataIndex: 'OrderEasyName',
            width: 100
        }, {
            text: "停用碼",
            dataIndex: 'OrderDCFlag',
            width: 100
        }, {
            text: "一次極量",
            dataIndex: 'MaxQtyPerTime',
            width: 100
        }, {
            text: "一日極量",
            dataIndex: 'MaxQtyPerDay',
            width: 100
        }, {
            text: "限制次數",
            dataIndex: 'MaxTakeTimes',
            width: 100
        }, {
            text: "衛生署核准字號",
            dataIndex: 'DOHLicenseNo',
            width: 100
        }, {
            text: "RFID條碼",
            dataIndex: 'RFIDCode',
            width: 100
        }, {
            text: "預設給藥途徑",
            dataIndex: 'PathNo',
            width: 100
        }, {
            text: "累計用藥",
            dataIndex: 'AggregateCode',
            width: 100
        }, {
            text: "開立限制",
            dataIndex: 'LimitFlag',
            width: 100
        }, {
            text: "管制用藥",
            dataIndex: 'RestrictCode',
            width: 100
        }, {
            text: "抗生素等級",
            dataIndex: 'AntibioticsCode',
            width: 100
        }, {
            text: "住院消耗歸整",
            dataIndex: 'CarryKindI',
            width: 100
        }, {
            text: "門急消耗歸整",
            dataIndex: 'CarryKindO',
            width: 100
        }, {
            text: "UD磨粉",
            dataIndex: 'UDPowderFlag',
            width: 100
        }, {
            text: "合理回流藥",
            dataIndex: 'ReturnDrugFlag',
            width: 100
        }, {
            text: "研究用藥",
            dataIndex: 'ResearchDrugFlag',
            width: 100
        }, {
            text: "藥包機品項",
            dataIndex: 'MachineFlag',
            width: 100
        }, {
            text: "限制途徑",
            dataIndex: 'FixPathNoFlag',
            width: 100
        }, {
            text: "適應症(中文)",
            dataIndex: 'SymptomChin',
            width: 100
        }, {
            text: "適應症(英文)",
            dataIndex: 'SymptomEng',
            width: 100
        }, {
            text: "不可剝半",
            dataIndex: 'OnlyRoundFlag',
            width: 100
        }, {
            text: "不可磨粉",
            dataIndex: 'UnablePowderFlag',
            width: 100
        }, {
            text: "高警訊藥品",
            dataIndex: 'DangerDrugFlag',
            width: 100
        }, {
            text: "高警訊藥品提示",
            dataIndex: 'DangerDrugMemo',
            width: 100
        }, {
            text: "冷藏存放",
            dataIndex: 'ColdStorageFlag',
            width: 100
        }, {
            text: "避光存放",
            dataIndex: 'LightAvoidFlag',
            width: 100
        }, {
            text: "異動狀態",
            dataIndex: 'ChangeStatus',
            width: 100
        }, {
            text: "門診給藥頻率",
            dataIndex: 'FreqNoO',
            width: 100
        }, {
            text: "住院給藥頻率",
            dataIndex: 'FreqNoI',
            width: 100
        }, {
            text: "預設開立天數",
            dataIndex: 'OrderDays',
            width: 100
        }, {
            text: "預設劑量",
            dataIndex: 'Dose',
            width: 100
        }, {
            text: "院內費用類別",
            dataIndex: 'HospChargeID1',
            width: 100
        }, {
            text: "醫令類別",
            dataIndex: 'OrderType',
            width: 100
        }, {
            text: "醫令類別(申報定義)",
            dataIndex: 'OrderKind',
            width: 100
        }, {
            text: "高價用藥",
            dataIndex: 'HighPriceFlag',
            width: 100
        }, {
            text: "住院醫囑顯示",
            dataIndex: 'InpDisplayFlag',
            width: 100
        }, {
            text: "替代院內代碼1",
            dataIndex: 'Substitute1',
            width: 100
        }, {
            text: "替代院內代碼2",
            dataIndex: 'Substitute2',
            width: 100
        }, {
            text: "替代院內代碼3",
            dataIndex: 'Substitute3',
            width: 100
        }, {
            text: "替代院內代碼4",
            dataIndex: 'Substitute4',
            width: 100
        }, {
            text: "替代院內代碼5",
            dataIndex: 'Substitute5',
            width: 100
        }, {
            text: "體重及安全量：計算別",
            dataIndex: 'WeightType',
            width: 100
        }, {
            text: "體重及安全量：限制數量",
            dataIndex: 'WeightUnitLimit',
            width: 100
        }, {
            text: "限制狀態",
            dataIndex: 'RestrictType',
            width: 100
        }, {
            text: "門診限制開立數量",
            dataIndex: 'MaxQtyO',
            width: 100
        }, {
            text: "住院限制開立數量",
            dataIndex: 'MaxQtyI',
            width: 100
        }, {
            text: "門診限制開立日數",
            dataIndex: 'MaxDaysO',
            width: 100
        }, {
            text: "住院限制開立日數",
            dataIndex: 'MaxDaysI',
            width: 100
        }, {
            text: "門診效期日數",
            dataIndex: 'ValidDaysO',
            width: 100
        }, {
            text: "住院效期日數",
            dataIndex: 'ValidDaysI',
            width: 100
        }, {
            text: "醫令排序",
            dataIndex: 'OrderCodeSort',
            width: 100
        }, {
            text: "藥品成份1",
            dataIndex: 'DrugElemCode1',
            width: 100
        }, {
            text: "藥品成份2",
            dataIndex: 'DrugElemCode2',
            width: 100
        }, {
            text: "藥品成份3",
            dataIndex: 'DrugElemCode3',
            width: 100
        }, {
            text: "藥品成份4",
            dataIndex: 'DrugElemCode4',
            width: 100
        }, {
            text: "TDM 藥品",
            dataIndex: 'TDMFlag',
            width: 100
        }, {
            text: "外審(健保專案)用藥",
            dataIndex: 'SpecialOrderKind',
            width: 100
        }, {
            text: "處置需報部位",
            dataIndex: 'NeedRegionFlag',
            width: 100
        }, {
            text: "醫令使用狀態",
            dataIndex: 'OrderUseType',
            width: 100
        }, {
            text: "預設劑量",
            dataIndex: 'FixDoseFlag',
            width: 100
        }, {
            text: "罕見疾病用藥",
            dataIndex: 'RareDisorderFlag',
            width: 100
        }, {
            text: "內審用藥",
            dataIndex: 'HospExamineFlag',
            width: 100
        }, {
            text: "給付條文代碼",
            dataIndex: 'OrderCondCode',
            width: 100
        }, {
            text: "內審限制用量",
            dataIndex: 'HospExamineQtyFlag',
            width: 100
        }, {
            text: "記錄建立日期/時間",
            dataIndex: 'CreateDateTime',
            width: 100
        }, {
            text: "記錄建立人員",
            dataIndex: 'CreateOpID',
            width: 100
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {

                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                viewport.down('#form').expand();
                if (T1LastRec) {
                    msglabel("");
                }
            }
        }
    });
    
    var T2Store = Ext.create('WEBAPP.store.AB0089_2', { // 定義於/Scripts/app/store/AB0089_2.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').rawValue,
                    p3: T1Query.getForm().findField('P3').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export2', text: '匯出', handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: 'BASORDD(價錢、合約…相關).xls' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').rawValue });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').rawValue });
                    PostForm(T2GetExcel, p);
                    msglabel('匯出完成');
                }
            }
        ]
    });

    // 查詢結果資料列表
    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: T2Name,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            /*dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {*/
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        },
        {
            text: "院內代碼",
            dataIndex: 'OrderCode',
            width: 100
        }, {
            text: "記錄處理日期/時間",
            dataIndex: 'ProcDateTime',
            width: 200
        }, {
            text: "記錄處理人員",
            dataIndex: 'ProcOpID',
            width: 100
        }, {
            text: "生效起日(開始日期)",
            dataIndex: 'BeginDate',
            width: 100
        }, {
            text: "生效迄日(結束日期)",
            dataIndex: 'EndDate',
            width: 100
        }, {
            text: "門診扣庫轉換量",
            dataIndex: 'StockTransQtyO',
            width: 100,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "住院扣庫轉換量",
            dataIndex: 'StockTransQtyI',
            width: 100,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "門診申報轉換量",
            dataIndex: 'ChangeQtyI',
            width: 100,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "住院申報轉換量",
            dataIndex: 'ChangeQtyO',
            width: 100,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "健保價",
            dataIndex: 'Price1',
            width: 100,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "自費價",
            dataIndex: 'Price2',
            width: 100,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "進價",
            dataIndex: 'CostAmount',
            width: 100,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "是否優惠",
            dataIndex: 'IsDisc',
            width: 100
        }, {
            text: "優惠%",
            dataIndex: 'DiscPer',
            width: 100
        }, {
            text: "健保碼",
            dataIndex: 'InsuOrderCode',
            width: 100
        }, {
            text: "健保負擔碼(住院)",
            dataIndex: 'InsuSignI',
            width: 100
        }, {
            text: "健保負擔碼(門診)",
            dataIndex: 'InsuSignO',
            width: 100
        }, {
            text: "合約單價",
            dataIndex: 'ContractPrice',
            width: 100,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "合約碼",
            dataIndex: 'ContracNo',
            width: 100
        }, {
            text: "廠商代碼(供應商代碼)",
            dataIndex: 'SupplyNo',
            width: 100
        }, {
            text: "標案來源",
            dataIndex: 'CaseFrom',
            width: 100
        }, {
            text: "製造廠名稱",
            dataIndex: 'OriginalProducer',
            width: 100
        }, {
            text: "申請商名稱",
            dataIndex: 'AgentName',
            width: 100
        }, {
            text: "記錄建立日期/時間",
            dataIndex: 'CreateDateTime',
            width: 100
        }, {
            text: "記錄建立人員",
            dataIndex: 'CreateOpID',
            width: 100
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                viewport.down('#form').expand();
                if (T2LastRec) {
                    msglabel("");
                }
            }
        }
    });
    
    var T3Store = Ext.create('WEBAPP.store.AB0089_3', { // 定義於/Scripts/app/store/AB0089_3.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P2的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').rawValue,
                    p3: T1Query.getForm().findField('P3').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
            itemId: 'export3', text: '匯出', handler: function () {
                var p = new Array();
                p.push({ name: 'FN', value: 'STKDMIT(藥衛材基本資料).xls' });
                p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue });
                p.push({ name: 'p2', value: T1Query.getForm().findField('P2').rawValue });
                p.push({ name: 'p3', value: T1Query.getForm().findField('P3').rawValue });
                PostForm(T3GetExcel, p);
                msglabel('匯出完成');
            }
        }
        ]
    });

    // 查詢結果資料列表
    var T3Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            /*dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {*/
            dock: 'top',
            xtype: 'toolbar',
            items: [T3Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        },
        {
            text: "藥品藥材代碼",
            dataIndex: 'SkOrderCode',
            width: 100
        }, {
            text: "記錄處理日期/時間",
            dataIndex: 'ProcDateTime',
            width: 100
        }, {
            text: "記錄處理人員",
            dataIndex: 'ProcOpID',
            width: 100
        }, {
            text: "藥品停用通知",
            dataIndex: 'DCMassageCode',
            width: 100
        }, {
            text: "藥品進貨異動",
            dataIndex: 'DMITDCCode',
            width: 100
        }, {
            text: "廠牌",
            dataIndex: 'Manufacturer',
            width: 100
        }, {
            text: "移動平均加權價weighted CostAmount",
            dataIndex: 'WCostAmount',
            width: 100,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "公藥分類(三總藥學專用欄位)",
            dataIndex: 'PublicDrugCode',
            width: 100
        }, {
            text: "扣庫規則分類(三總藥學專用欄位)",
            dataIndex: 'StockUseCode',
            width: 100
        }, {
            text: "規格量及單位",
            dataIndex: 'SpecNUnit',
            width: 100
        }, {
            text: "成份量及單位",
            dataIndex: 'ComponentNUnit',
            width: 100
        }, {
            text: "軍聯項次年號",
            dataIndex: 'YearArmyNo',
            width: 100
        }, {
            text: "軍聯項次號",
            dataIndex: 'ItemArmyNo',
            width: 100
        }, {
            text: "軍聯項次分類",
            dataIndex: 'GroupArmyNo',
            width: 100
        }, {
            text: "軍聯項次組別",
            dataIndex: 'ClassifiedArmyNo',
            width: 100
        }, {
            text: "合約效期",
            dataIndex: 'ContractEffectiveDate',
            width: 100
        }, {
            text: "藥品單複方",
            dataIndex: 'MultiPrescriptionCode',
            width: 100
        }, {
            text: "藥品類別",
            dataIndex: 'DrugClass',
            width: 100
        }, {
            text: "藥品性質欄位(僅做藥品之分類，線上並無作用)",
            dataIndex: 'DrugClassify',
            width: 100
        }, {
            text: "藥品劑型",
            dataIndex: 'DrugForm',
            width: 100
        }, {
            text: "藥委會註記",
            dataIndex: 'CommitteeMemo',
            width: 100
        }, {
            text: "藥委會品項",
            dataIndex: 'CommitteeCode',
            width: 100
        }, {
            text: "盤點品項 Y/N",
            dataIndex: 'InventoryFlag',
            width: 100
        }, {
            text: "院內單位",
            dataIndex: 'ApplyUnit',
            width: 100
        }, {
            text: "藥品採購案別",
            dataIndex: 'PurchaseCaseType',
            width: 100
        }, {
            text: "TDM 合理治療濃度上限",
            dataIndex: 'MaxCureConsistency',
            width: 100
        }, {
            text: "TDM 合理治療濃度下限",
            dataIndex: 'MinCureConsistency',
            width: 100
        }, {
            text: "TDM 合理PEAK起",
            dataIndex: 'PearBegin',
            width: 100
        }, {
            text: "TDM 合理PEAK迄",
            dataIndex: 'PearEnd',
            width: 100
        }, {
            text: "TDM 合理 Trough 起",
            dataIndex: 'TroughBegin',
            width: 100
        }, {
            text: "TDM 合理 Trough 迄",
            dataIndex: 'TroughEnd',
            width: 100
        }, {
            text: "TDM 危急值 起",
            dataIndex: 'DangerBegin',
            width: 100
        }, {
            text: "TDM 危急值 迄",
            dataIndex: 'DangerEnd',
            width: 100
        }, {
            text: "TDM 備註1",
            dataIndex: 'TDMMemo1',
            width: 100
        }, {
            text: "TDM 備註2",
            dataIndex: 'TDMMemo2',
            width: 100
        }, {
            text: "TDM 備註3",
            dataIndex: 'TDMMemo3',
            width: 100
        }, {
            text: "注意事項(中文)",
            dataIndex: 'ChinAttention',
            width: 100
        }, {
            text: "注意事項(英文)",
            dataIndex: 'EngAttentio',
            width: 100
        }, {
            text: "處方集",
            dataIndex: 'DrugMemo',
            width: 100
        }, {
            text: "主要副作用(中文)",
            dataIndex: 'ChinSideEffect',
            width: 100
        }, {
            text: "主要副作用(英文)",
            dataIndex: 'EngSideEffect',
            width: 100
        }, {
            text: "警語",
            dataIndex: 'Warn',
            width: 100
        }, {
            text: "衛生署核准適應症",
            dataIndex: 'DOHSymptom',
            width: 100
        }, {
            text: "FDA核准適應症",
            dataIndex: 'FDASymptom',
            width: 100
        }, {
            text: "授乳安全性",
            dataIndex: 'SuckleSecurity',
            width: 100
        }, {
            text: "懷孕分級",
            dataIndex: 'PregnantGrade',
            width: 100
        }, {
            text: "藥品外觀",
            dataIndex: 'DrugExterior',
            width: 100
        }, {
            text: "採購單位",
            dataIndex: 'PurchaseUnit',
            width: 100
        }, {
            text: "藥品異動備註",
            dataIndex: 'DCMassageMemo',
            width: 100
        }, {
            text: "第一次進貨日期",
            dataIndex: 'FirstPurchaseDate',
            width: 100
        }, {
            text: "藥品仿單檔名",
            dataIndex: 'DrugLeafletLink',
            width: 100
        }, {
            text: "藥品圖片檔名",
            dataIndex: 'DrugPictureLink',
            width: 100
        }, {
            text: "成份量及單位2",
            dataIndex: 'ComponentNUnit2',
            width: 100
        }, {
            text: "成份量及單位3",
            dataIndex: 'ComponentNUnit3',
            width: 100
        }, {
            text: "成份量及單位4",
            dataIndex: 'ComponentNUnit4',
            width: 100
        }, {
            text: "藥品外觀(英文)",
            dataIndex: 'DrugEngExterior',
            width: 100
        }, {
            text: "軍品院內碼",
            dataIndex: 'ArmyOrderCode',
            width: 100
        }, {
            text: "藥品請領類別",
            dataIndex: 'DrugApplyType',
            width: 100
        }, {
            text: "母藥註記",
            dataIndex: 'ParentCode',
            width: 100
        }, {
            text: "母藥院內碼",
            dataIndex: 'ParentOrderCode',
            width: 100
        }, {
            text: "子藥轉換量",
            dataIndex: 'SonTransQty',
            width: 100
        }, {
            text: "記錄建立日期/時間",
            dataIndex: 'CreateDateTime',
            width: 100
        }, {
            text: "記錄建立人員",
            dataIndex: 'CreateOpID',
            width: 100
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                }
            },
            selectionchange: function (model, records) {
                T3Rec = records.length;
                T3LastRec = records[0];
                if (T3LastRec) {
                    msglabel("");
                }
            }
        }
    });
    
    
    var TATabs = Ext.widget('tabpanel', {
        listeners: {
            tabchange: function (tabpanel, newCard, oldCard) {
                switch (newCard.title) {
                    case "BASORDM(醫囑線上使用)":
                        T1Query.getForm().findField('P0').focus();
                        break;
                    case "BASORDD(價錢、合約…相關)":
                        T1Query.getForm().findField('P2').focus();
                        T1Query.getForm().findField('P0').clearInvalid();
                        break;
                    case "STKDMIT(藥衛材基本資料)":
                        T1Query.getForm().findField('P3').focus();
                        T1Query.getForm().findField('P0').clearInvalid();
                        break;
                }
            }
        },
        layout: 'fit',
        plain: true,
        border: false,
        resizeTabs: true,       //改變tab尺寸       
        enableTabScroll: true,  //是否允許Tab溢出時可以滾動
        defaults: {
            // autoScroll: true,
            closabel: false,    //tab是否可關閉
            padding: 0,
            split: true
        },
        items: [{
            itemId: 't1Grid',
            title: 'BASORDM(醫囑線上使用)',
            layout: 'border',
            padding: 0,
            split: true,
            region: 'center',
            layout: 'fit',
            collapsible: false,
            border: false,
            items: [T1Grid]
        }, {
            itemId: 't2Grid',
            title: 'BASORDD(價錢、合約…相關)',
            layout: 'border',
            padding: 0,
            split: true,
            region: 'center',
            layout: 'fit',
            collapsible: false,
            border: false,
            items: [T2Grid]
        }, {
            itemId: 't3Grid',
            title: 'STKDMIT(藥衛材基本資料)',
            layout: 'border',
            padding: 0,
            split: true,
            region: 'center',
            layout: 'fit',
            collapsible: false,
            border: false,
            items: [T3Grid]
        }]
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Query',
            region: 'north',
            layout: 'fit',
            items: [T1Query]
        },{
            itemId: 't1Form',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [TATabs]
        }
        ]
    });
    
    function T1Load() {
        T1Tool.moveFirst();
    }
    function T2Load() {
        T2Tool.moveFirst();
    }
    function T3Load() {
        T3Tool.moveFirst();
    }
    T1Query.getForm().findField('P0').focus(); //讓游標停在P0這一格

});
