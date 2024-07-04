Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除

    var T1RecLength = 0;
    var T1LastRec = null;

    var user_kind = '';
    var user_task = '';
    var wh_no = '';
    var mm_code = '';
    var winAA0129;

    function getUserKind() {
        Ext.Ajax.request({
            url: '/api/AA0129/GetUserkind',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                user_kind = data;
            },
            failure: function (response, options) {

            }
        });
    }
    getUserKind();

    function getUsertask() {
        Ext.Ajax.request({
            url: '/api/AA0069/GetUsertask',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                user_task = data;
            },
            failure: function (response, options) {

            }
        });
    }
    getUsertask();
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var St_MatclassGet = '../../../api/AA0069/GetMatclassCombo';
    var MMCODEComboGet = '../../../api/AA0061/GetMMCODECombo';


    var reportUrl = '/Report/A/AA0061.aspx';　　　　　　　　//明細

    // 物料群組
    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    var st_mat_class = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0069/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_doc_type = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0069/GetDoctypeCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_mcode = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0069/GetMcodeCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var WhnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM'],
        proxy: {
            type: 'ajax',
            url: '/api/AA0069/GetWhnoCombo',
            reader: {
                type: 'json',
                root: 'etts'
            }
        },
        autoLoad: false
    });

    var wh_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AA0069/GetWhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: user_kind
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store) {
                store.insert(0, { TEXT: '', VALUE: '', COMBITEM: '' });
            }
        },
        autoLoad: true
    });

    var mat_class_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AA0129/GetMAT_CLASSCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            beforeload: function (store, options) {
                var wh_kind = '';
                var index = wh_store.find('VALUE', T1Query.getForm().findField('P3').getValue());
                if (index != -1)
                    wh_kind = wh_store.data.items[wh_store.find('VALUE', T1Query.getForm().findField('P3').getValue())].data.COMBITEM
                var np = {
                    p0: wh_kind,
                    user_kind: user_kind
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store) {
                store.insert(0, { TEXT: '', VALUE: '' });
            }
        },
        autoLoad: true
    });

    var st_matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0069/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var GradeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['KEY_CODE', 'COMBITEM'],
        data: [
            { "KEY_CODE": "1", "COMBITEM": "1 庫" },
            { "KEY_CODE": "2", "COMBITEM": "2 局" },
            { "KEY_CODE": "3", "COMBITEM": "3 病房" },
            { "KEY_CODE": "4", "COMBITEM": "4 科室" }

        ]
    });




    function setComboData() {

        //物料分類
        Ext.Ajax.request({
            url: St_MatclassGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_matclass = data.etts;
                    //T1Query.getForm().findField('SHOWOPT').setValue({ SHOWOPTRB: 1 });   // 初始化RB
                    //T1Query.getForm().findField('SHOWDATA').setValue({ SHOWDATARB: 1 }); // 初始化RB


                    if (tb_matclass.length > 0) {
                        for (var i = 0; i < tb_matclass.length; i++) {
                            st_matclass.add({ VALUE: tb_matclass[i].VALUE, COMBITEM: tb_matclass[i].COMBITEM });
                        }

                        if (tb_matclass.length == 1) {
                            //1筆資料時將
                            T1Query.getForm().findField('P0').setValue(tb_matclass[0].VALUE);
                        }
                        else {
                            T1Query.getForm().findField('P0').setDisabled(false);
                        }
                    }

                }
            },
            failure: function (response, options) {

            }
        });
    }


    //Ext.Ajax.request({
    //    url: '/api/AA0069/GetWhnoCombo',
    //    method: reqVal_p,
    //    success: function (response) {
    //        var data = Ext.decode(response.responseText);
    //        if (data.success) {
    //            var tb_vendor = data.etts;
    //            if (tb_vendor.length > 0) {
    //                for (var i = 0; i < tb_vendor.length; i++) {
    //                    WhnoQueryStore.add({ VALUE: tb_vendor[i].VALUE, COMBITEM: tb_vendor[i].COMBITEM });
    //                }
    //            }
    //        }
    //    },
    //    failure: function (response, options) {

    //    }
    //});

    setComboData();

    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P1',
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: 60,
        width: 238,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0069/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                mat_class: T1Query.getForm().findField('P0').getValue()
                
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
        padding: '0 4 0 4'
    });

    // 查詢欄位

    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    var mLabelWidth = 60;
    var mWidth = 180;
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
                    fieldLabel: '交易日期',
                    name: 'D0',
                    id: 'D0',
                    width: 140,
                    vtype: 'dateRange',
                    dateRange: { end: 'D1' },
                    padding: '0 4 0 4',
                    allowBlank: false,
                    fieldCls: 'required',
                    value: getToday()
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'D1',
                    id: 'D1',
                    labelWidth: 7,
                    width: 90,
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'D0' },
                    padding: '0 4 0 4',
                    allowBlank: false,
                    fieldCls: 'required',
                    value: getToday()
                }
                //, {
                //    xtype: 'radiogroup',
                //    anchor: '40%',
                //    labelWidth: 20,
                //    width: 100,
                //    //name: 'KINDDATA',
                //    items: [
                //        { boxLabel: '藥品', width: 50, name: 'KINDDATARB', inputValue: 0, checked: true },
                //        { boxLabel: '衛材', width: 50, name: 'KINDDATARB', inputValue: 1 }
                //    ],
                //    listeners:
                //    {
                //        change: function (rg, nVal, oVal, eOpts) {
                //            //setVisibleColumns(nVal.SHOWOPT);
                //            showkind(nVal.KINDDATARB)
                //        }
                //    }

                //}
                , {
                    xtype: 'hidden',
                    fieldLabel: '庫房級別',
                    name: 'P2',
                    id: 'P2'
                    //,
                    //store: GradeQueryStore,
                    //queryMode: 'local',
                    //displayField: 'COMBITEM',
                    //valueField: 'KEY_CODE',
                    //queryMode: 'local',
                    //anyMatch: true,
                    //autoSelect: true,
                    //listeners: {
                    //    beforequery: function (record) {
                    //        record.query = new RegExp(record.query, 'i');
                    //        record.forceAll = true;
                    //    },
                    //    select: function (combo, records, eOpts) {
                    //        var values = combo.up('form').getValues();
                    //        var category = values.KINDDATARB;
                    //        var level = values.P2;
                    //        WhnoQueryStore.load({
                    //            params: {
                    //                category: category,
                    //                level: level
                    //            }
                    //        });
                    //    }
                    //}
                }, {
                    xtype: 'combo',
                    fieldLabel: '庫房',
                    id: 'P3',
                    queryMode: 'local',
                    store: wh_store,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    matchFieldWidth: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    padding: '0 4 0 4',
                    listConfig: {
                        width: 230
                    },
                    listeners: {
                        select: function (combo, record, index) {
                            if (user_kind == "S") {
                                Ext.getCmp('P4').setValue("");
                                Ext.getCmp('P4').setRawValue("");
                                mat_class_store.load();
                            }
                        }
                    }
                },{
                    id: 'P0',
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    queryMode: 'local',
                    //store: st_matclass,
                    store: st_mat_class,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    padding: '0 4 0 4',
                    allowBlank: false,
                    forceSelection: true,
                    fieldCls: 'required'
                }
            ]
            }, {
                    xtype: 'panel',
                    border: false,
                    layout: 'hbox',
                    items: [
                        
                //        {
                //    xtype: 'radiogroup',
                //    anchor: '40%',
                //    labelWidth: 25,
                //    width: 110,
                //    name: 'SHOWOPT',
                //    items: [
                //        { boxLabel: '庫備', width: 50, name: 'SHOWOPTRB', inputValue: 1 },
                //        { boxLabel: '非庫備', width: 60, name: 'SHOWOPTRB', inputValue: 0 }
                //    ],
                //    listeners:
                //    {
                //        beforequery: function (record) {
                //            record.query = new RegExp(record.query, 'i');
                //            record.forceAll = true;
                //            Ext.Msg.alert('說明', T1Query.getForm().findField('P0').getValue());
                //        },
                //        change: function (rg, nVal, oVal, eOpts) {   //mat_class 物料分類的值, nVal['SHOWOPT'] 庫備識別碼(0非庫備, 1庫備)

                //        }
                //    }
                //},
                 T1QuryMMCode,
                {
                    id: 'P9',
                    xtype: 'combo',
                    fieldLabel: '表單類別',
                    queryMode: 'local',
                    store: st_doc_type,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    padding: '0 4 0 4'
                },
                {
                    id: 'P10',
                    xtype: 'combo',
                    fieldLabel: '庫存異動',
                    queryMode: 'local',
                    store: st_mcode,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    padding: '0 4 0 4'
                }
            ],
            }, {
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'textfield',
                        id: 'P11',
                        enforceMaxLength: true,
                        maxLength: 21,
                        width: 240,
                        fieldLabel: '表單號碼',
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: T1Load,
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            T1Query.getForm().findField('D0').reset();
                            T1Query.getForm().findField('D1').reset();
                            T1Query.getForm().findField('P0').reset();
                            T1Query.getForm().findField('P1').reset();
                            T1Query.getForm().findField('P2').reset();
                            T1Query.getForm().findField('P3').reset();
                            T1Query.getForm().findField('P9').reset();
                            T1Query.getForm().findField('P10').reset();
                            T1Query.getForm().findField('P11').reset();
                            if (user_task == '1') {
                                T1Query.getForm().findField('P0').setValue('01');
                            }
                            else {
                                T1Query.getForm().findField('P0').setValue('02');
                            }
                            var f = this.up('form').getForm();
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在D0
                            msglabel('訊息區:');
                        }
                    }
                ],
            }],

    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['WH_NO', 'MMCODE', 'MMNAME_C', 'MMNAME_E', 'TR_DATE,', 'TR_DOCNO',
            'TR_INV_QTY', 'TR_ONWAY_QTY', 'DOCTYPE_NAME', 'MCODE_NAME', 'BASE_UNIT',
            'M_CONTPRICE', 'LOTNO_EXP_QTY']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });

    var T1Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,

        sorters: [{ property: 'TR_SNO', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    //p0: T1QueryForm.getForm().findField('P0').getValue(),
                    p0: T1Query.getForm().findField('D0').rawValue,
                    p1: T1Query.getForm().findField('D1').rawValue,
                    p2: T1Query.getForm().findField('P0').getValue(),
                    p3: T1Query.getForm().findField('P1').getValue(),
                    p4: T1Query.getForm().findField('P2').getValue(),
                    p5: T1Query.getForm().findField('P3').getValue(),
                    p6: '',
                    p7: T1Query.getForm().getValues()['SHOWOPTRB'],
                    //p8: T1QueryForm.getForm().findField('P8').getValue()
                    p9: T1Query.getForm().findField('P9').getValue(),
                    p10: T1Query.getForm().findField('P10').getValue(),
                    p11: T1Query.getForm().findField('P11').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            timeout: 90000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0069/GetAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });


    function T1Load() {

        T1Store.load({
            params: {
                start: 0
            }
        });
        T1Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        //buttons: [
        //    {
        //        itemId: 'export', text: '匯出', //T1Query
        //        handler: function () {
        //            var p = new Array();

        //            if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "1") {
        //                p.push({ name: 'FN', value: '明細報表' });
        //            } else if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "2") {
        //                p.push({ name: 'FN', value: '彙總報表' });
        //            }

        //            if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue != "") {
        //                p.push({ name: 'd0', value: T1Query.getForm().findField('D0').getValue() });
        //                p.push({ name: 'd1', value: T1Query.getForm().findField('D1').getValue() });
        //                p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
        //                p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
        //                p.push({ name: 'p2', value: T1Query.getForm().findField('SHOWOPT').getChecked()[0].inputValue });
        //                p.push({ name: 'p3', value: T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue });

        //                PostForm(T1GetExcel, p);
        //                msglabel('訊息區:匯出完成');
        //            } else {
        //                //msglabel('訊息區:需要選匯出選項' + T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue);
        //                msglabel('訊息區:');
        //            }

        //        }
        //    },
        //    {
        //        text: '列印', handler: function () {
        //            if (T1Store.getCount() > 0) {
        //                //msglabel(T1Query.getForm().findField('D0').getValue() + "," + T1Query.getForm().findField('D1').getValue());

        //                if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "1") {        //明細
        //                    reportUrl = '/Report/A/AA0061.aspx';
        //                    showReport();
        //                } else if (T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue == "2") { //彙整
        //                    reportUrl = '/Report/A/AA0061General.aspx';
        //                    showReport();
        //                }
        //            }
        //            else
        //                Ext.Msg.alert('訊息', '請先建立明細資料');
        //        }
        //    },

        //]
    });
    function showkind(value) {
        console.log(value)
        //alert(T1Query.getForm().getValues()['KINDDATARB'])
        //alert(T1Query.getForm().findField('KINDDATARB').getChecked()[0].inputValue)
        if (value == 0) {
            GradeQueryStore.loadData([
                { "KEY_CODE": "1", "COMBITEM": "1 庫" },
                { "KEY_CODE": "2", "COMBITEM": "2 局" },
                { "KEY_CODE": "3", "COMBITEM": "3 病房" },
                { "KEY_CODE": "4", "COMBITEM": "4 科室" }
            ]);
        }
        else {
            GradeQueryStore.loadData([
                { "KEY_CODE": "1", "COMBITEM": "1 庫" },
                { "KEY_CODE": "2", "COMBITEM": "2 衛星庫" },
            ]);
        }
    }
    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?APPTIME1=' + T1Query.getForm().findField('D0').getValue()
                + '&APPTIME2=' + T1Query.getForm().findField('D1').getValue()
                + '&task_id=' + T1Query.getForm().findField('P0').getValue()
                + '&mmcode=' + T1Query.getForm().findField('P1').getValue()
                + '&showopt=' + T1Query.getForm().findField('SHOWOPT').getChecked()[0].inputValue
                + '&showdata=' + T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]     //新增 修改功能畫面
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "流水號",
                dataIndex: 'TR_SNO',
                style: 'text-align:left',
                align: 'left',
                width: 90
            }, {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 70
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                style: 'text-align:left',
                align: 'left',
                width: 200
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                style: 'text-align:left',
                align: 'left',
                width: 250
            }, {
                text: "交易時間",
                dataIndex: 'TR_DATE',
                style: 'text-align:left',
                align: 'left',
                width: 120
            }, {
                text: "表單號碼",
                dataIndex: 'TR_DOCNO',
                style: 'text-align:left',
                align: 'left',
                width: 150
            }, {
                text: "異動數量",
                dataIndex: 'TR_INV_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 70,
                renderer: function (val, meta, record) {
                    return Ext.util.Format.number(val, "0,000");
                }
            }, {
                text: "異動在途量",
                dataIndex: 'TR_ONWAY_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 70,
                renderer: function (val, meta, record) {
                    return Ext.util.Format.number(val, "0,000");
                }
            }, {
                text: "表單類別",
                dataIndex: 'DOCTYPE_NAME',
                style: 'text-align:left',
                align: 'left',
                width: 150
            }, {
                text: "庫存異動",
                dataIndex: 'MCODE_NAME',
                style: 'text-align:left',
                align: 'left',
                width: 70
            }, {
                text: "異動前庫存量",
                dataIndex: 'BF_TR_INVQTY',
                style: 'text-align:left',
                align: 'right',
                width: 100,
                renderer: function (val, meta, record) {
                    return Ext.util.Format.number(val, "0,000") ;
                }
            }, {
                text: "異動後庫存量",
                dataIndex: 'AF_TR_INVQTY',
                style: 'text-align:left',
                align: 'right',
                width: 100,
                renderer: function (val, meta, record) {
                    return Ext.util.Format.number(val, "0,000");
                }
            }, {
                text: "出庫庫房",
                dataIndex: 'FRWH_N',
                style: 'text-align:left',
                align: 'left',
                width: 150
            }, {
                text: "入庫庫房",
                dataIndex: 'TOWH_N',
                style: 'text-align:left',
                align: 'left',
                width: 150
            }, {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                style: 'text-align:left',
                align: 'left',
                width: 70

            }, {
                text: "合約單價",
                dataIndex: 'M_CONTPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 80,
                //renderer: function (val, meta, record) {
                //    //return Ext.util.Format.number(val, "0,000.00");
                //}

            }, {
                text: "批號-效期",
                dataIndex: 'LOTNO_EXP_QTY',
                style: 'text-align:left',
                align: 'left',
                width: 300

            },{
                header: "",
                flex: 1
            }],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
            }
        }
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
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        },
        ]
    });

    T1Query.getForm().findField('D0').focus();
    if (user_task == '1') {
        T1Query.getForm().findField('P0').setValue('01');
    }
    else {
        T1Query.getForm().findField('P0').setValue('02');
    }
});

//alert(T1Query.getForm().findField('KINDDATA').getValue(SHOWDATARB))







//Ext.Loader.setConfig({
//    enabled: true,
//    paths: {
//        'WEBAPP': '/Scripts/app'
//    }
//});

////搜尋院內碼
//var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
//    name: 'P1',
//    id: 'P1',
//    name: 'MMCODE',
//    fieldLabel: '院內碼',


//    //width: 150,

//    //限制一次最多顯示10筆
//    limit: 10,

//    //指定查詢的Controller路徑
//    queryUrl: '/api/AA0092/GetMMCodeCombo',

//    //查詢完會回傳的欄位
//    extraFields: ['MAT_CLASS', 'BASE_UNIT'],
//    listeners: {
//        select: function (c, r, i, e) {
//            //選取下拉項目時，顯示回傳值

//        }
//    }
//});

//var mmCodeCombo1 = Ext.create('WEBAPP.form.MMCodeCombo', {
//    name: 'P2',
//    id: 'P2',
//    name: 'MMCODE',
//    fieldLabel: '至    ',


//    //width: 150,

//    //限制一次最多顯示10筆
//    limit: 10,

//    //指定查詢的Controller路徑
//    queryUrl: '/api/AA0092/GetMMCodeCombo',

//    //查詢完會回傳的欄位
//    extraFields: ['MAT_CLASS', 'BASE_UNIT'],
//    listeners: {
//        select: function (c, r, i, e) {
//            //選取下拉項目時，顯示回傳值

//        }
//    }
//});

//var WHQueryStore = Ext.create('Ext.data.Store', {
//    fields: ['KEY_CODE', 'COMBITEM']
//});


//Ext.onReady(function () {
//    var T1Set = ''; // 新增/修改/刪除
//    var T1Name = "";

//    var T1Rec = 0;
//    var T1LastRec = null;

//    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

//    function setComboData() {
//        Ext.Ajax.request({
//            url: '/api/AA0089/GetWHClassCombo',
//            method: reqVal_p,
//            success: function (response) {
//                var data = Ext.decode(response.responseText);

//                if (data.success) {
//                    var tb_vendor = data.etts;
//                    console.log(tb_vendor)
//                    if (tb_vendor.length > 0) {
//                        for (var i = 0; i < tb_vendor.length; i++) {
//                            WHQueryStore.add({ KEY_CODE: tb_vendor[i].KEY_CODE, COMBITEM: tb_vendor[i].COMBITEM });
//                        }
//                    }
//                }
//            },
//            failure: function (response, options) {

//            }
//        });


//    }
//    setComboData();

//    // 查詢欄位
//    var mLabelWidth = 60;
//    var mWidth = 180;
//    var T1Query = Ext.widget({
//        xtype: 'form',
//        layout: 'form',
//        border: false,
//        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
//        fieldDefaults: {
//            xtype: 'textfield',
//            labelAlign: 'right',
//            labelWidth: mLabelWidth,
//            width: mWidth
//        },
//        items: [{
//            xtype: 'panel',
//            id: 'PanelP1',
//            border: false,
//            layout: 'hbox',
//            items: [
//                {
//                    xtype: 'combobox',
//                    fieldLabel: '庫別代碼',
//                    name: 'P0',
//                    id: 'P0',
//                    store: WHQueryStore,
//                    displayField: 'COMBITEM',
//                    valueField: 'KEY_CODE',
//                    queryMode: 'local',
//                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
//                    width: 250,
//                    allowBlank: false, // 欄位是否為必填
//                    fieldCls: 'required',
//                    blankText: "請選擇庫房別",
//                    padding: '0 4 0 4'
//                }, mmCodeCombo,
//                {
//                    xtype: 'label',
//                    id: 'P3',
//                    name: 'P3',
//                    fieldLabel: '至',
//                    padding: '0 4 0 4'


//                }, mmCodeCombo1
//                , {
//                    xtype: 'combobox',
//                    fieldLabel: '各庫停用碼',
//                    name: 'P4',
//                    id: 'P4',
//                    queryMode: 'local',
//                    displayField: 'name',
//                    valueField: 'abbr',
//                    width: 200,
//                    labelWidth: 100,
//                    store: [
//                        { abbr: '', name: '全部' },
//                        { abbr: '0', name: '正常使用' },
//                        { abbr: '1', name: '刪除' },
//                        { abbr: '2', name: '停產' },
//                        { abbr: '3', name: '廠缺' },
//                        { abbr: '4', name: '變更' },
//                    ],
//                    value: '',
//                    padding: '0 4 0 4'
//                }, {
//                     xtype: 'combobox',
//                    fieldLabel: '類別',
//                    name: 'P5',
//                    id: 'P5',
//                    queryMode: 'local',
//                    displayField: 'name',
//                    valueField: 'abbr',
//                    width: 145,
//                    store: [
//                        { abbr: '', name: '全部' },
//                        { abbr: 'Y', name: '是' },
//                        { abbr: 'N', name: '否' }
//                    ],
//                    value: '',
//                    padding: '0 4 0 4'

//                },
//                {
//                    xtype: 'button',
//                    text: '查詢',
//                    handler: function () {

//                        T1Load();
//                    }
//                }, {
//                    xtype: 'button',
//                    text: '清除',
//                    handler: function () {
//                        var f = this.up('form').getForm();
//                        f.reset();
//                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
//                    }
//                }


//            ]
//        }]
//    });

//    Ext.define('T1Model', {
//        extend: 'Ext.data.Model',
//        fields: ['MMCODE', 'MMNAME_E', 'INV_QTY,', 'DAVG_USEQTY', 'SAFE_DAY', 'SAFE_QTY', 'OPER_DAY', 'OPER_QTY', 'LOW_QTY', 'STORE_LOC', 'EXP_DATE', 'BASE_UNIT','E_SUPSTATUS']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

//    });

//    var T1Store = Ext.create('Ext.data.Store', {
//        // autoLoad:true,
//        model: 'T1Model',
//        pageSize: 20,
//        remoteSort: true,

//        sorters: [{ property: 'MMCODE', direction: 'ASC' }],

//        listeners: {
//            beforeload: function (store, options) {
//                var np = {
//                    //p0: T1QueryForm.getForm().findField('P0').getValue(),
//                    p0: T1Query.getForm().findField('P0').getValue(),
//                    p1: T1Query.getForm().findField('P1').getValue(),
//                    p2: T1Query.getForm().findField('P2').getValue(),
//                    p4: T1Query.getForm().findField('P4').getValue(),
//                    p5: T1Query.getForm().findField('P5').getValue(),
//                    //p6: T1QueryForm.getForm().findField('P6').getValue(),
//                    //p7: T1QueryForm.getForm().findField('P7').getValue(),
//                    //p8: T1QueryForm.getForm().findField('P8').getValue()
//                };
//                Ext.apply(store.proxy.extraParams, np);
//            }
//        },

//        proxy: {
//            type: 'ajax',
//            timeout: 90000,
//            actionMethods: {
//                read: 'POST' // by default GET
//            },
//            url: '/api/AA0089/GetAll',
//            reader: {
//                type: 'json',
//                rootProperty: 'etts',
//                totalProperty: 'rc'
//            }
//        }

//    });


//    function T1Load() {

//        T1Store.load({
//            params: {
//                start: 0
//            }
//        });
//    }
//    // toolbar,包含換頁、新增/修改/刪除鈕
//    var T1Tool = Ext.create('Ext.PagingToolbar', {
//        store: T1Store,
//        displayInfo: true,
//        border: false,
//        plain: true,
//        buttons: [
//            {
//                itemId: 'export', text: '匯出', disabled: false,
//                handler: function () {
//                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
//                        if (btn === 'yes') {

//                            PostForm(T1GetExcel, p);
//                        }
//                    });
//                }
//            }
//        ]
//    });

//    // 查詢結果資料列表
//    var T1Grid = Ext.create('Ext.grid.Panel', {
//        title: T1Name,
//        store: T1Store,
//        plain: true,
//        loadingText: '處理中...',
//        loadMask: true,
//        cls: 'T1',
//        dockedItems: [{
//            dock: 'top',
//            xtype: 'toolbar',
//            layout: 'fit',
//            items: [T1Query]
//        }, {
//            dock: 'top',
//            xtype: 'toolbar',
//            items: [T1Tool]
//        }],
//        columns: [
//            {
//                xtype: 'rownumberer'
//            },{
//                text: "藥品院內碼",
//                dataIndex: 'MMCODE',
//                width: 110
//            }, {
//                text: "名稱",
//                dataIndex: 'MMNAME_E',
//                width: 280
//            }, {
//                text: "現有存量",
//                dataIndex: 'INV_QTY',
//                style: 'text-align:left',
//                align: 'right',
//                width: 70
//            }, {
//                text: "日平均耗量",
//                dataIndex: 'DAVG_USEQTY',
//                style: 'text-align:left',
//                align: 'right',
//                width: 70
//            }, {
//                text: "安全天數",
//                dataIndex: 'A.SAFE_DAY',
//                width: 60
//            }, {
//                text: "安全存量",
//                dataIndex: 'A.SAFE_QTY',
//                style: 'text-align:left',
//                align: 'right',
//                width: 70
//            }, {
//                text: "作業天數",
//                dataIndex: 'A.OPER_DAY',
//                style: 'text-align:left',
//                align: 'right',
//                width: 70
//            }, {
//                text: "作業量",
//                dataIndex: 'A.OPER_QTY',
//                style: 'text-align:left',
//                align: 'right',
//                width: 70
//            }, {
//                text: "最低庫存量",
//                dataIndex: 'LOW_QTY,',
//                style: 'text-align:left',
//                align: 'right',
//                width: 70
//            }, {
//                text: "STORE_LOC",
//                dataIndex: 'STORE_LOC',
//                style: 'text-align:left',
//                align: 'right',
//                width: 70
//            }, {
//                text: "有效日期",
//                dataIndex: 'EXP_DATE',
//                style: 'text-align:left', 
//                align: 'right',
//                width: 70
//            }, {
//                text: "計量單位",
//                dataIndex: 'BASE_UNIT',
//                style: 'text-align:left', 
//                align: 'right',
//                width: 70
//            }, {
//                text: "停用碼",
//                dataIndex: 'E_SUPSTATUS',
//                style: 'text-align:left', 
//                align: 'right',
//                width: 70
//            }, {
//                header: "",
//                flex: 1
//            }],
//        viewConfig: {
//            listeners: {
//                refresh: function (view) {
//                    T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
//                }
//            }
//        },
//        listeners: {
//            selectionchange: function (model, records) {
//                T1Rec = records.length;
//                T1LastRec = records[0];
//            }
//        }
//    });




//    var viewport = Ext.create('Ext.Viewport', {
//        renderTo: body,
//        layout: {
//            type: 'border',
//            padding: 0
//        },
//        defaults: {
//            split: true
//        },
//        items: [{
//            itemId: 't1Grid',
//            region: 'center',
//            layout: 'fit',
//            collapsible: false,
//            title: '',
//            border: false,
//            items: [T1Grid]
//        }
//        ]
//    });

//    //var d = new Date();
//    //m = d.getMonth(); //current month
//    //y = d.getFullYear(); //current year

//    //T1Query.getForm().findField('P0').focus();
//    //T1Query.getForm().findField('P3').setValue(new Date(y, m, 1));
//    //T1Query.getForm().findField('P4').setValue(d);

//});