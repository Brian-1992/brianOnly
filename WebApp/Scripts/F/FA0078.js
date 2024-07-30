Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

var T1Name = "採購月結報表";
var T1GetTxt = '../../../api/FA0078/GetTxt';

// 物料類別清單
var mat_classQueryStore = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT']
});

// 是否合庫
var st_IsTCB = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT'],
    data: [
        { "VALUE": "0", "TEXT": "合庫" },
        { "VALUE": "1", "TEXT": "非合庫" },
        { "VALUE": "", "TEXT": "不區分" }
    ]
});

var T1GetExcel = '/api/FA0078/Excel';
var reportUrl = '/Report/F/FA0078.aspx';
var reportUrl2 = '/Report/F/FA0078_2.aspx';
var reportUrl3 = '/Report/F/FA0078_3.aspx';

var P0 = '';
var P1 = '';
var P2 = '';
var IsTCB = '';


Ext.onReady(function () {

    function setComboData() {
        Ext.Ajax.request({
            url: '/api/FA0078/Mat_ClassComboGet',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var dt_matclass = data.etts;
                    if (dt_matclass.length > 0) {
                        for (var i = 0; i < dt_matclass.length; i++) {
                            mat_classQueryStore.add({ VALUE: dt_matclass[i].VALUE, TEXT: dt_matclass[i].TEXT });
                        }
                        T1Query.getForm().findField('MAT_CLASS').setValue(dt_matclass[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 180;
    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [{
                xtype: 'combo',
                fieldLabel: '物料類別',
                name: 'MAT_CLASS',
                enforceMaxLength: true,
                labelWidth: 60,
                width: 170,
                padding: '0 4 0 4',
                store: mat_classQueryStore,
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                anyMatch: true,
                fieldCls: 'required',
                allowBlank: false,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
            },
            {
                xtype: 'combobox',
                fieldLabel: '報表類別',
                name: 'ExportType',
                queryMode: 'local',
                displayField: 'name',
                valueField: 'abbr',
                width: 145,
                labelWidth: 60,
                editable: false,
                store: [
                    { abbr: 'Y', name: '零購' },
                    { abbr: 'N', name: '合約' },
                    { abbr: '', name: '不區分' }
                ],
                fieldCls: 'required',
                allowBlank: false,
                padding: '0 4 0 4',
                value: 'N'
            }, {
                xtype: 'monthfield',
                fieldLabel: '月結月份',
                name: 'YYYMM',
                labelWidth: 60,
                width: 130,
                padding: '0 4 0 4',
                fieldCls: 'required',
                allowBlank: false
            }, {
                xtype: 'combo',
                fieldLabel: '合庫否',
                store: st_IsTCB,
                name: 'IsTCB',
                id: 'IsTCB',
                labelWidth: 65,
                width: 170,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                value: '',
                fieldCls: 'required',
                allowBlank: false
            },
            {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    if (T1Query.getForm().isValid()) {

                        var f = T1Query.getForm();
                        P0 = f.findField('MAT_CLASS').getValue();
                        P1 = T1Query.getForm().findField('ExportType').getValue();
                        P2 = T1Query.getForm().findField('YYYMM').rawValue;
                        IsTCB = T1Query.getForm().findField('IsTCB').getValue();
                        T1Load();
                    }
                    else {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>請輸入必填欄位</span>');
                    }

                }
            },
            {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    //f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    msglabel('訊息區:');
                }
            }
            ]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'AGEN_NO', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'AGEN_ACC', type: 'string' },
            { name: 'TOT_AMT', type: 'string' },
            { name: 'TOT_AMT_1', type: 'string' },
            { name: 'TOT_AMT_2', type: 'string' },
            { name: 'TOT_AMT_3', type: 'string' },
            { name: 'TOT_AMT_4', type: 'string' }
        ]

    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'AGEN_NO', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0078/All',
            timeout: 900000,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    P0: P0,
                    P1: P1,
                    P2: P2,
                    IsTCB: IsTCB
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯入,列印是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    T1Tool.down('#export').setDisabled(false);
                    T1Tool.down('#t1print').setDisabled(false);
                    T1Tool.down('#t2print').setDisabled(false);
                    T1Tool.down('#t3print').setDisabled(false);
                    if (T1Query.getForm().findField('IsTCB').getValue() == '0' ||
                        T1Query.getForm().findField('IsTCB').getValue() == '1') {
                        T1Tool.down('#exportTXT').setDisabled(false);
                    }
                    else
                        T1Tool.down('#exportTXT').setDisabled(true);
                } else {
                    T1Tool.down('#export').setDisabled(true);
                    T1Tool.down('#t1print').setDisabled(true);
                    T1Tool.down('#t2print').setDisabled(true);
                    T1Tool.down('#t3print').setDisabled(true);
                    T1Tool.down('#exportTXT').setDisabled(true);
                }
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }
    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'P0', value: P0 }); //SQL篩選條件
                            p.push({ name: 'P1', value: P1 }); //SQL篩選條件
                            p.push({ name: 'P2', value: P2 }); //SQL篩選條件
                            p.push({ name: 'IsTCB', value: IsTCB }); //SQL篩選條件                            
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            },
            {
                id: 't1print', text: '支付廠商明細', disabled: true, handler: function () {
                    showReport();
                }
            },
            {
                id: 't2print', text: '訂購清冊明細', disabled: true, handler: function () {
                    showReport2('');
                }
            },
            {
                id: 't3print', text: '對帳發票明細', disabled: true, handler: function () {
                    showReport3();
                }
            },
            {
                text: '匯出TXT',
                id: 'exportTXT',
                name: 'exportTXT',
                style: 'margin:0px 5px;',
                disabled: true,
                handler: function () {
                    P0 = T1Query.getForm().findField('MAT_CLASS').getValue();
                    var P0Name = T1Query.getForm().findField('MAT_CLASS').rawValue;
                    P1 = T1Query.getForm().findField('ExportType').getValue();
                    P2 = T1Query.getForm().findField('YYYMM').rawValue;
                    IsTCB = T1Query.getForm().findField('IsTCB').getValue();
                    var p = new Array();
                    p.push({ name: 'p0', value: P0 });
                    p.push({ name: 'p0_Name', value: P0Name });
                    p.push({ name: 'p1', value: P1 });
                    p.push({ name: 'p2', value: P2 });
                    p.push({ name: 'p3', value: IsTCB });
                    PostForm(T1GetTxt, p);
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
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
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
                text: '明細',
                align: 'center',
                stopSelection: true,
                width: 50,
                xtype: 'widgetcolumn',
                widget: {
                    xtype: 'button',
                    _btnText: "明細",
                    defaultBindProperty: null, //important
                    handler: function (widgetColumn) {
                        var record = widgetColumn.getWidgetRecord();
                        showReport2(record.data.AGEN_NO);
                    },
                    listeners: {
                        beforerender: function (widgetColumn) {
                            var record = widgetColumn.getWidgetRecord();
                            widgetColumn.setText(widgetColumn._btnText); //can be mixed with the row data if needed
                        }
                    }
                }
            },
            {
                text: "廠商編號",
                dataIndex: 'AGEN_NO',
                width: 85
            }, {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 250
            }, {
                text: "銀行帳號",
                dataIndex: 'AGEN_ACC',
                width: 120
            }, {
                text: "應付總價",
                dataIndex: 'TOT_AMT',
                style: 'text-align:left',
                align: 'right',
                width: 110
            }, {
                text: "折讓金額",
                dataIndex: 'TOT_AMT_1',
                style: 'text-align:left',
                align: 'right',
                width: 110
            }, {
                text: "聯標契約優惠",
                dataIndex: 'TOT_AMT_2',
                style: 'text-align:left',
                align: 'right',
                width: 130
            }, {
                text: "額外折讓金額",
                dataIndex: 'TOT_AMT_3',
                style: 'text-align:left',
                align: 'right',
                width: 130
            }, {
                text: "實付總價",
                dataIndex: 'TOT_AMT_4',
                style: 'text-align:left',
                align: 'right',
                width: 110
            }, {
                header: "",
                flex: 1
            }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
            }
        }
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                    + '?P0=' + P0
                    + '&P1=' + P1
                    + '&P2=' + P2
                    + '&IsTCB=' + IsTCB
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

    function showReport2(AGEN_NO) {
        var win = Ext.create('Ext.window.Window', {
            title: '明細',
            layout: 'fit',    //設定佈局模式為fit，能讓frame自適應窗體大小
            modal: true,    //開啟遮罩層
            height: '90%',    //初始高度
            width: '90%',  //初始寬度
            border: 0,    //無邊框
            frame: false,    //去除窗體的panel框架
            html: '<iframe src="' + reportUrl2
                + '?P0=' + P0
                + '&P1=' + P1
                + '&P2=' + P2
                + '&P5=' + AGEN_NO
                + '&IsTCB=' + IsTCB
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
            buttons: [{
                text: '關閉',
                handler: function () {
                    this.up('window').destroy();
                }
            }]
        });
        win.show();    //顯示視窗
    }
    
    function showReport3() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl3
                    + '?P0=' + P0
                    + '&P1=' + P1
                    + '&P2=' + P2
                    + '&IsTCB=' + IsTCB
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
        }
        ]
    });

    var d = new Date();
    m = d.getMonth(); //current month
    y = d.getFullYear(); //current year
    T1Query.getForm().findField('YYYMM').setValue(new Date(y, m));
});
