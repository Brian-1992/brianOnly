Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

var matUserID;


var T1GetExcel = '/api/AA0068/Excel';
var reportUr = '/Report/A/AA0068.aspx';
var reportUr1 = '/Report/B/BD0010.aspx';

var P0 = '';
var P1 = '';
var P2 = '';
var P3 = '';
var P4 = '';

// 資料自1090801起

// 物品類別清單
var Wh_noQueryStore = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT']
});

// 排除廠商清單
var Agen_noQueryStore = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT']
});

// 報表類別
var reportQueryStore = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT']
});

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "";

    var T1Rec = 0;
    var T1LastRec = null;

    function setComboData() {
        Ext.Ajax.request({
            url: '/api/AA0068/GetWH_NoCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_no = data.etts;
                    if (wh_no.length > 0) {
                        for (var i = 0; i < wh_no.length; i++) {
                            Wh_noQueryStore.add({ VALUE: wh_no[i].VALUE, TEXT: wh_no[i].TEXT });
                        }
                        T1Query.getForm().findField('WH_NO').setValue(wh_no[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    function setAgenComboData() {
        Ext.Ajax.request({
            url: '/api/AA0068/GetAgen_NoCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var agen_no = data.etts;
                    if (agen_no.length > 0) {
                        for (var i = 0; i < agen_no.length; i++) {
                            Agen_noQueryStore.add({ VALUE: agen_no[i].VALUE, TEXT: agen_no[i].TEXT });
                        }
                        
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setAgenComboData();

    function getHospCode() {
        Ext.Ajax.request({
            url: '/api/AA0068/GetHospCode',
            method: reqVal_p,
            success: function (response) {
                Ext.getCmp('t1print1').show();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg != '0') {
                        Ext.getCmp('t1print1').hide();

                        reportQueryStore.add({ VALUE: '全部', TEXT: '全部' });
                        reportQueryStore.add({ VALUE: '合約', TEXT: '合約' });
                        reportQueryStore.add({ VALUE: '非合約', TEXT: '非合約' });

                        T1Query.getForm().findField('P3').setValue('全部');

                        T1Query.getForm().findField('AGEN_NO').setValue('');

                    }
                    else {
                        reportQueryStore.add({ VALUE: '全部', TEXT: '全部' });
                        reportQueryStore.add({ VALUE: '合約', TEXT: '合約' });
                        reportQueryStore.add({ VALUE: '零購', TEXT: '零購' });

                        T1Query.getForm().findField('AGEN_NO').setValue('300');
                    }
                    
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getHospCode();

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });



    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        //autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '庫房代碼',
                    name: 'WH_NO',
                    id: 'WH_NO',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 170,
                    store: Wh_noQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '進貨日期',
                    name: 'P1',
                    labelWidth: mLabelWidth,
                    width: 150,
                    fieldCls: 'required',
                    allowBlank: false,
                    value: new Date()
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P2',
                    labelWidth: 10,
                    width: 100,
                    fieldCls: 'required',
                    allowBlank: false,
                    labelSeparator: '&nbsp',
                    value: new Date()
                },
                //{
                //    xtype: 'combobox',
                //    fieldLabel: '報表類別',
                //    name: 'P3',
                //    id: 'P3',
                //    queryMode: 'local',
                //    displayField: 'abbr',
                //    valueField: 'abbr',
                //    labelWidth: 60,
                //    width: 120,
                //    editable: false,
                //    store: [
                //        { abbr: '全部' },
                //        { abbr: '合約' },
                //        { abbr: '零購' }
                //        //{ abbr: '零購_0Y' }
                //        //{ abbr: '零購_0N' }
                //    ],
                //    value: '全部'
                //},
                {
                    xtype: 'combo',
                    fieldLabel: '報表類別',
                    name: 'P3',
                    id: 'P3',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 120,
                    store: reportQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    value: '全部'
                },
                {
                    xtype: 'combo',
                    fieldLabel: '排除廠商',
                    name: 'AGEN_NO',
                    id: 'AGEN_NO',
                    multiSelect: true,
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 220,
                    matchFieldWidth: false,
                    listConfig: { width: 280 },
                    store: Agen_noQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,                   
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = T1Query.getForm();
                        if (f.isValid()) {
                            P0 = f.findField('WH_NO').getValue();
                            P1 = f.findField('P1').rawValue;
                            P2 = f.findField('P2').rawValue;
                            P3 = f.findField('P3').getValue();
                            P4 = f.findField('AGEN_NO').getValue();

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
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('WH_NO').focus(); // 進入畫面時輸入游標預設在P0
                    }
                //}, {
                //    xtype: 'datefield',
                //    fieldLabel: '採購日期',
                //    name: 'P5',
                //    enforceMaxLength: true,
                //    labelWidth: 60,
                //    width: 140,
                //    padding: '0 4 0 0',
                //    allowBlank: false,
                //    value: new Date(),
                //    fieldCls: 'required'
                //}
                }, {
                    xtype: 'datefield',
                    fieldLabel: '採購日期',
                    name: 'YYYYMMDD',
                    enforceMaxLength: true,
                    maxLength: 7,
                    labelWidth: 70,
                    width: 150,
                    allowBlank: false,
                    fieldCls: 'required',
                    regexText: '請選擇日期',
                    value: new Date()
                }, {
                    xtype: 'datefield',
                    fieldLabel: '迄',
                    name: 'YYYYMMDD_E',
                    enforceMaxLength: true,
                    maxLength: 7,
                    labelWidth: 20,
                    width: 100,
                    allowBlank: false,
                    fieldCls: 'required',
                    regexText: '請選擇日期',
                    value: new Date()
                }
            ]
        }
        ]
    });

    var T1Store = Ext.create('WEBAPP.store.AA.AA0068', { // 定義於/Scripts/app/store/AA/AA0092.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    P0: P0,
                    P1: P1,
                    P2: P2,
                    P3: P3,
                    P4: P4
                };
                Ext.apply(store.proxy.extraParams, np);

            },
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
                itemId: 'export', text: '匯出進貨報表', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'P0', value: P0 }); //SQL篩選條件
                            p.push({ name: 'P1', value: P1 }); //SQL篩選條件
                            p.push({ name: 'P2', value: P2 }); //SQL篩選條件
                            p.push({ name: 'P3', value: P3 }); //SQL篩選條件
                            p.push({ name: 'P4', value: P4 }); //SQL篩選條件
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            }, {
                itemId: 't1print', text: '列印進貨報表', disabled: false, handler: function () {
                    showReport();
                }
            }, {
                itemId: 't1print1', id:'t1print1', text: '列印申購報表', disabled: false, handler: function () {
                    P0 = T1Query.getForm().findField('WH_NO').getValue();
                    showReport1();
                }
            },
            { xtype: 'tbtext', text: '資料自1090801起' }
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
            }, {
                text: "進貨日期",
                dataIndex: 'ACCOUNTDATE',
                width: 80
            }, {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO',
                width: 140
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            }, {
                text: "藥品名稱",
                dataIndex: 'MMNAME_E',
                width: 260
            }, {
                text: "單位",
                dataIndex: 'M_PURUN',
                width: 50
            }, {
                text: "發票單價",
                dataIndex: 'PO_PRICE',
                style: 'text-align:right',
                align: 'right',
                width: 100
            }, {
                text: "類別",
                dataIndex: 'FLAG',
                style: 'text-align:center',
                align: 'center',
                width: 45,
                renderer: function (val, meta, record) {
                    if (val == 'D退貨')
                        return '<font color="blue"><b>' + val + '</b></font>';
                    else
                        return val;
                }
            }, {
                text: "進貨量",
                dataIndex: 'DELI_QTY',
                style: 'text-align:right',
                align: 'right',
                width: 90
            }, {
                text: "發票金額",
                dataIndex: 'PO_AMT',
                style: 'text-align:right',
                align: 'right',
                width: 90
            }, {
                text: "說明",
                dataIndex: 'MEMO',
                align: 'left',
                width: 50
            }, {
                text: "折讓金額",
                dataIndex: 'DISC_AMT',
                style: 'text-align:right',
                align: 'right',
                width: 90
            }, {
                text: "單次訂購達優惠數量折讓金額",
                dataIndex: 'WILL_DISC_AMT',
                style: 'text-align:right',
                align: 'right',
                width: 180
            }, {
                text: "優惠價",
                dataIndex: 'DISC_CPRICE',
                style: 'text-align:right',
                align: 'right',
                width: 90
            }, {
                text: "優惠金額",
                dataIndex: 'PAY_AMT',
                style: 'text-align:right',
                align: 'right',
                width: 90
            }, {
                header: "",
                flex: 1
            }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
                    T1Tool.down('#t1print').setDisabled(T1Store.getCount() === 0);
                }
            }
        },
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
                html: '<iframe src="' + reportUr
                + '?P0=' + P0
                + '&P1=' + P1
                + '&P2=' + P2
                + '&P3=' + P3
                + '&P4=' + P4
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
    function showReport1() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUr1  
                + '?WH_NO=' + P0
                + '&YYYYMMDD=' + T1Query.getForm().findField('YYYYMMDD').rawValue
                + '&YYYYMMDD_E=' + T1Query.getForm().findField('YYYYMMDD_E').rawValue
                + '&PO_STATUS=0&Agen_No=&RptFrom=AA0068-1&CONTRACT=' + T1Query.getForm().findField('P3').getValue()
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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


});


