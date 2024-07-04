Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

var agenComboGet = '../../../api/AA0085/GetAgenCombo';
var T1GetTxt = '../../../api/AA0085/GetTxt';

// 廠商清單
var agen_noQueryStore = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT']
});

// 庫別清單
var wh_noQueryStore = Ext.create('Ext.data.Store', {
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

var T1GetExcel = '/api/AA0085/Excel';
var reportUrl = '/Report/A/AA0085.aspx';
var reportUrl2 = '/Report/A/AA0085_2.aspx';

var P0 = '';
var P1 = '';
var P2 = '';
var P3 = '';
var P4 = '';
var IsTCB = '';


Ext.onReady(function () {

    function setComboData2() {
        Ext.Ajax.request({
            url: agenComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            agen_noQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                        T1Query.getForm().findField('AGEN_NO').setValue('300');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData2();

    function setComboData3() {
        Ext.Ajax.request({
            url: '/api/AA0085/Wh_NoComboGet',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            wh_noQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                        T1Query.getForm().findField('WH_NO').setValue(wh_nos[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData3();

    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "";

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
                fieldLabel: '庫別代碼',
                name: 'WH_NO',
                enforceMaxLength: true,
                labelWidth: 60,
                width: 170,
                padding: '0 4 0 4',
                store: wh_noQueryStore,
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
                    { abbr: 'N', name: '合約' }
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
                fieldLabel: '排除廠商',
                name: 'AGEN_NO',
                enforceMaxLength: true,
                labelWidth: 60,
                width: 220,
                padding: '0 4 0 4',
                store: agen_noQueryStore,
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                anyMatch: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
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
                value: ''
            },
            {
                xtype: 'checkboxfield',
                boxLabel: '<span style="color:blue">明細</span>',
                padding: '0 4 0 4',
                name: 'P4',
                inputValue: '1'
            },
            {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    if (T1Query.getForm().isValid()) {

                        var f = T1Query.getForm();
                        P0 = f.findField('WH_NO').getValue();
                        P1 = T1Query.getForm().findField('ExportType').getValue();
                        P2 = T1Query.getForm().findField('YYYMM').rawValue;
                        P3 = T1Query.getForm().findField('AGEN_NO').getValue();
                        P4 = f.findField('P4').checked;
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

    var T1Store = Ext.create('WEBAPP.store.AA.AA0085', { // 定義於/Scripts/app/store/AA/AA0092.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    P0: P0,
                    P1: P1,
                    P2: P2,
                    P3: P3,
                    IsTCB: IsTCB
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯入,列印是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    T1Tool.down('#export').setDisabled(false);
                    T1Tool.down('#t1print').setDisabled(false);
                    if (T1Query.getForm().findField('IsTCB').getValue() == '0' ||
                        T1Query.getForm().findField('IsTCB').getValue() == '1') {
                        T1Tool.down('#exportTXT').setDisabled(false);
                    }
                } else {
                    T1Tool.down('#export').setDisabled(true);
                    T1Tool.down('#t1print').setDisabled(true);
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
                            p.push({ name: 'P3', value: P3 }); //SQL篩選條件
                            p.push({ name: 'IsTCB', value: IsTCB }); //SQL篩選條件                            
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            },
            {
                id: 't1print', text: '列印', disabled: true, handler: function () {
                    showReport();
                    if (T1Query.getForm().findField('P4').checked) {
                        showReport2();
                    }
                }
            },
            {
                text: '匯出TXT',
                id: 'exportTXT',
                name: 'exportTXT',
                style: 'margin:0px 5px;',
                disabled: true,
                handler: function () {
                    P0 = T1Query.getForm().findField('WH_NO').getValue();
                    P1 = T1Query.getForm().findField('ExportType').getValue();
                    P2 = T1Query.getForm().findField('YYYMM').rawValue;
                    P3 = T1Query.getForm().findField('AGEN_NO').getValue();
                    IsTCB = T1Query.getForm().findField('IsTCB').getValue();
                    var p = new Array();
                    p.push({ name: 'P0', value: P0 });
                    p.push({ name: 'P1', value: P1 });
                    p.push({ name: 'P2', value: P2 });
                    p.push({ name: 'P3', value: P3 });
                    p.push({ name: 'IsTCB', value: IsTCB });
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
                        showReport3(record.data.AGEN_NO);
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
                text: "公司名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 250
            }, {
                text: "合庫",
                dataIndex: 'AGEN_ISLOCAL',
                width: 50
            }, {
                text: "銀行帳號",
                dataIndex: 'AGEN_ACC',
                width: 120
            }, {
                text: "發票金額",
                dataIndex: 'TOT_AMT',
                style: 'text-align:left',
                align: 'right',
                width: 120
            }, {
                text: "備註",
                dataIndex: 'MEMO',
                width: 90
            }, {
                text: "發票張數",
                dataIndex: '',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "折讓金額",
                dataIndex: 'TOT_AMT_1',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "單次訂購達優惠數量折扣金額",
                dataIndex: 'TOT_AMT_2',
                style: 'text-align:left',
                align: 'right',
                width: 100
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
                + '&P3=' + P3
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

    function showReport2(w, h, x, y) {
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
            + '&P3=' + P3
            + '&P4=' + P4
            + '&P5=' + ''
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

    function showReport3(AGEN_NO) {
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
            + '&P3=' + P3
            + '&P4=' + P4
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
