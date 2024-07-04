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

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var St_MatclassGet = '../../../api/FA0037/GetMatclassCombo';

    var T1GetExcel = '../../../api/FA0037/Excel';
    var T2GetExcel = '../../../api/FA0037/Excel2'; //匯出非庫備申請單
    var reportUrl = '/Report/F/FA0037.aspx';
    var reportUr2 = '/Report/F/FA0037_2.aspx';
    var reportUr3 = '/Report/F/FA0037_3.aspx';

    // var reportUrl = '/Report/F/FA0037.aspx';　　　　　　　　//明細

    // 物料群組
    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });


    var GradeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['KEY_CODE', 'COMBITEM'],
        data: [
            { "KEY_CODE": "0", "COMBITEM": "0 非庫備" },
            { "KEY_CODE": "1", "COMBITEM": "1 庫備" },
            { "KEY_CODE": "", "COMBITEM": "不區分" }
        ]
    });

    var ContidQueryStore = Ext.create('Ext.data.Store', {
        fields: ['KEY_CODE', 'COMBITEM'],
        data: [
            { "KEY_CODE": "0", "COMBITEM": "0 合約" },
            { "KEY_CODE": "2", "COMBITEM": "2 非合約" },
            { "KEY_CODE": "", "COMBITEM": "不區分" }

        ]
    });
    
    var XactionStore = Ext.create('Ext.data.Store', {
        fields: ['KEY_CODE', 'COMBITEM'],
        data: [
            { "KEY_CODE": "0", "COMBITEM": "0 臨時申購" },
            { "KEY_CODE": "1", "COMBITEM": "1 常態申購" },
            { "KEY_CODE": "", "COMBITEM": "不區分" }

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

                        if (tb_matclass.length > 0) {
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

    setComboData();

    function SetDate() {
        nowDate = new Date();
        nowDate.getMonth();
        nowDate = Ext.Date.format(nowDate, "Ymd") - 19110000;
        nowDate = nowDate.toString().substring(0, 5);
        T1Query.getForm().findField('P1').setValue(nowDate);
    }


    // 查詢欄位
    var mLabelWidth = 55;
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
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    name: 'P0',
                    id: 'P0',
                    store: st_matclass,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    // multiSelect: true,
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: false,
                    fieldCls: 'required',
                    autoSelect: true,
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {

                        }
                    },

                }, {
                    xtype: 'datefield',
                    fieldLabel: '日期',
                    name: 'D0',
                    id: 'D0',
                    allowBlank: false,
                    fieldCls: 'required',
                    width: 145,
                    value: Ext.util.Format.date(new Date(), "Y-m-") + "01"
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'D1',
                    id: 'D1',
                    labelWidth: 7,
                    allowBlank: false,
                    fieldCls: 'required',
                    width: 100,
                    value: new Date()
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '月份別',
                    name: 'P1',
                    id: 'P1',
                    width: 130,
                    labelWidth: 60,
                    fieldCls: 'required',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    padding: '0 1 0 1',
                    fieldCls: 'required',
                    allowBlank: false
                }, {
                    xtype: 'combo',
                    fieldLabel: '庫備分類',
                    name: 'P2',
                    id: 'P2',
                    store: GradeQueryStore,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'KEY_CODE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: false,
                    value: '',
                    fieldCls: 'required',
                    width: 140,
                    autoSelect: true,
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {

                        }
                    },
                }, {
                    xtype: 'combo',
                    fieldLabel: '合約分類',
                    name: 'P3',
                    id: 'P3',
                    store: ContidQueryStore,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'KEY_CODE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: false,
                    fieldCls: 'required',
                    value: '',
                    width: 140,
                    autoSelect: true,
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {

                        }
                    },
                }, {
                    xtype: 'combo',
                    fieldLabel: '類別',
                    name: 'P4',
                    id: 'P4',
                    store: XactionStore,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'KEY_CODE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: false,
                    fieldCls: 'required',
                    value: '',
                    labelWidth: 45,
                    width: 140,
                    autoSelect: true,
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {

                        }
                    },
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = T1Query.getForm();
                        if (f.isValid()) {
                            T1Load();
                        }
                        else {

                            Ext.MessageBox.alert('提示', '請輸入必填欄位');
                        }
                    }
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
                        T1Query.getForm().findField('P4').reset();
                        var f = this.up('form').getForm();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在D0
                        msglabel('訊息區:');
                    }
                },

            ],
        }],

    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['PR_NO', 'MMCODE', 'MMNAME_C', 'MMNAME_E', 'AGEN_NO,', 'AGEN_NAMEC', 'AGEN_TEL', 'M_PURUN', 'M_AGENLAB', 'M_CONTPRICE', 'BASE_UNIT', 'M_CONTPRICE', 'M_PHCTNCO', 'M_ENVDT', 'DISC', 'M_CONTID', 'M_STROREID', 'REQ_QTY', 'TOT']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });

    var T1Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,

        sorters: [{ property: 'MMCODE', direction: 'ASC' }],

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
                    p6: T1Query.getForm().findField('P4').getValue(),
                    //p8: T1QueryForm.getForm().findField('P8').getValue()
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
            url: '/api/FA0037/GetAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });


    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't1print', text: '列印彙總表', disabled: false,
                handler: function () {
                    showReport();
                }
            }, {
                itemId: 'export', text: '匯出彙總表', disabled: false,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'P0', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件
                    p.push({ name: 'P1', value: T1Query.getForm().findField('P1').rawValue }); //SQL篩選條件
                    p.push({ name: 'P2', value: T1Query.getForm().findField('P2').getValue() }); //SQL篩選條件
                    p.push({ name: 'P3', value: T1Query.getForm().findField('P3').getValue() }); //SQL篩選條件
                    p.push({ name: 'P4', value: T1Query.getForm().findField('P4').getValue() }); //SQL篩選條件
                    p.push({ name: 'D0', value: T1Query.getForm().findField('D0').rawValue }); //SQL篩選條件
                    p.push({ name: 'D1', value: T1Query.getForm().findField('D1').rawValue }); //SQL篩選條件
                    //p.push({ name: 'P3', value: P3 }); //SQL篩選條件
                    PostForm(T1GetExcel, p);
                }
            }, {
                itemId: 'export1', text: '匯出非庫備申請單', disabled: false,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'P0', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件
                    p.push({ name: 'P1', value: T1Query.getForm().findField('P1').rawValue }); //SQL篩選條件
                    p.push({ name: 'P2', value: T1Query.getForm().findField('P2').getValue() }); //SQL篩選條件
                    p.push({ name: 'P3', value: T1Query.getForm().findField('P3').getValue() }); //SQL篩選條件
                    p.push({ name: 'P4', value: T1Query.getForm().findField('P4').getValue() }); //SQL篩選條件
                    p.push({ name: 'D0', value: T1Query.getForm().findField('D0').rawValue }); //SQL篩選條件
                    p.push({ name: 'D1', value: T1Query.getForm().findField('D1').rawValue }); //SQL篩選條件
                    PostForm(T2GetExcel, p);
                }
            }, {
                itemId: 't1print2', text: '列印申購報表', disabled: false,
                handler: function () {
                    showReport2();
                }
            }, {
                itemId: 't1print3', text: '列印申購總表', disabled: false,
                handler: function () {
                    showReport3();
                }
            }
        ]
    });

    function showReport() {
        if (!win) {
            var np = {
                p0: T1Query.getForm().findField('D0').rawValue,
                p1: T1Query.getForm().findField('D1').rawValue,
                p2: T1Query.getForm().findField('P0').getValue(),
                p3: T1Query.getForm().findField('P1').rawValue,
                p4: T1Query.getForm().findField('P2').getValue(),
                p5: T1Query.getForm().findField('P3').getValue(),
                p6: T1Query.getForm().findField('P4').getValue()
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                title: '申購彙總表',
                layout: 'fit',
                closable: false,
                //html: '<iframe src="' + reportUrl + '?WH_NO=' + WH_NO + '&STORE_LOC=' + STORE_LOC + '&BARCODE_IsUsing=' + BARCODE_IsUsing + '&STATUS=' + STATUS + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',

                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '&p4=' + np.p4 + '&p5=' + np.p5 + '&p6=' + np.p6 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    margin: '0 20 30 0',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

        }
        win.show();
    }
    function showReport2() {
        if (!win) {
            var np = {
                p0: T1Query.getForm().findField('D0').rawValue,
                p1: T1Query.getForm().findField('D1').rawValue,
                p2: T1Query.getForm().findField('P0').getValue(),
                p3: T1Query.getForm().findField('P1').rawValue,
                p4: T1Query.getForm().findField('P2').getValue(),
                p5: T1Query.getForm().findField('P3').getValue(),
                p6: T1Query.getForm().findField('P4').getValue()
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                title: '庫房申購資料報表',
                layout: 'fit',    //設定佈局模式為fit，能讓frame自適應窗體大小
                modal: true,    //開啟遮罩層
                height: '90%',    //初始高度
                width: '90%',  //初始寬度
                border: 0,    //無邊框
                frame: false,    //去除窗體的panel框架
                closable: false,
                html: '<iframe src="' + reportUr2 + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '&p4=' + np.p4 + '&p5=' + np.p5 + '&p6=' + np.p6 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    margin: '0 20 30 0',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

        }
        win.show();
    }
    function showReport3() {
        if (!win) {
            var np = {
                p0: T1Query.getForm().findField('D0').rawValue,
                p1: T1Query.getForm().findField('D1').rawValue,
                p2: T1Query.getForm().findField('P0').getValue(),
                p3: T1Query.getForm().findField('P1').rawValue,
                p4: T1Query.getForm().findField('P2').getValue(),
                p5: T1Query.getForm().findField('P3').getValue(),
                p6: T1Query.getForm().findField('P4').getValue()
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                title: '庫房申購總表',
                layout: 'fit',    //設定佈局模式為fit，能讓frame自適應窗體大小
                modal: true,    //開啟遮罩層
                height: '90%',    //初始高度
                width: '90%',  //初始寬度
                border: 0,    //無邊框
                frame: false,    //去除窗體的panel框架
                closable: false,
                html: '<iframe src="' + reportUr3 + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '&p4=' + np.p4 + '&p5=' + np.p5 + '&p6=' + np.p6 +'" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    margin: '0 20 30 0',
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
                text: "院內碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                style: 'text-align:left',
                align: 'left',
                width: 250
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                style: 'text-align:left',
                align: 'left',
                width: 250

            }, {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO',
                style: 'text-align:left',
                align: 'left',
                width: 60
            }, {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAMEC',
                style: 'text-align:left',
                align: 'left',
                width: 150
            }, {
                text: "環保證號",
                dataIndex: 'm_phctnco',
                style: 'text-align:left',
                align: 'left',
                width: 70
            }, {
                text: "折讓比",
                dataIndex: 'DISC',
                style: 'text-align:left',
                align: 'right',
                width: 60
            }, {
                text: "計量單位",
                dataIndex: 'M_PURUN',
                style: 'text-align:left',
                align: 'left',
                width: 60
            }, {
                text: "申請量",
                dataIndex: 'REQ_QTY_T',
                style: 'text-align:left',
                align: 'right',
                width: 70

            }, {
                text: "合約單價",
                dataIndex: 'M_CONTPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 80

            }, {
                text: "本月總價",
                dataIndex: 'TOT',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "申購單編號",
                dataIndex: 'PR_NO',
                style: 'text-align:left',
                align: 'left',
                width: 180
            }, {
                text: "緊急醫療出貨",
                dataIndex: 'ISCR',
                style: 'text-align:left',
                align: 'left',
                width: 120
            }, {
                header: "",
                flex: 1

            }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
                    T1Tool.down('#export1').setDisabled(T1Store.getCount() === 0);
                    T1Tool.down('#t1print').setDisabled(T1Store.getCount() === 0);
                    T1Tool.down('#t1print2').setDisabled(T1Store.getCount() === 0);
                    T1Tool.down('#t1print3').setDisabled(T1Store.getCount() === 0);
                }
            }
        },
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
    //T1Query.getForm().findField('D0').setValue(new Date());
    //T1Query.getForm().findField('D1').setValue(new Date());
    SetDate();
});