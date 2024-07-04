Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

var MATComboGet = '../../../api/FA0076/GetMATCombo';
var matUserID;
var today = new Date();
var y = today.getFullYear();
var m = today.getMonth();
var d = today.getDate();

var date_b = new Date(y, m, d - 7);
var date_e = new Date(y, m, d - 1);


// 物品類別清單
var MATQueryStore = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT']
});


function setComboData() {
    Ext.Ajax.request({
        url: MATComboGet,
        params: {
            p0: matUserID
        },
        method: reqVal_g,
        success: function (response) {
            var data = Ext.decode(response.responseText);
            if (data.success) {
                var wh_nos = data.etts;
                if (wh_nos.length > 0) {
                    for (var i = 0; i < wh_nos.length; i++) {
                        MATQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                    }
                }
            }
        },
        failure: function (response, options) {

        }
    });
}
setComboData();


var T1GetExcel = '/api/FA0076/Excel';

var P0 = '';
var P1 = '';
var P2 = '';
var P3 = '';
var P4 = '';
var P5 = '';
var P6 = '';
var P7 = '';

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });



    // 查詢欄位
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
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '物料類別',
                    name: 'P0',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 170,
                    padding: '0 4 0 4',
                    store: MATQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '日期區間',
                    name: 'P1',
                    labelWidth: mLabelWidth,
                    width: 150,
                    padding: '0 4 4 4',
                    fieldCls: 'required',
                    allowBlank: false,
                    value: date_b
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P2',
                    labelWidth: 8,
                    width: 88,
                    padding: '0 4 0 4',
                    labelSeparator: '',
                    fieldCls: 'required',
                    allowBlank: false,
                    value: date_e
                }
            ]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [{
                xtype: 'radiogroup',
                fieldLabel: '群組別',
                width: 230,
                labelWidth: 60,
                items: [
                    { boxLabel: '以成本碼', name: 'rb1', width: 70, inputValue: '1', checked: true },
                    { boxLabel: '以院內碼', name: 'rb1', width: 70, inputValue: '2' }
                ]
            }, {
                xtype: 'radiogroup',
                fieldLabel: '庫備類別',
                width: 300,
                labelWidth: 80,
                items: [
                    { boxLabel: '不區分', name: 'rb2', width: 70, inputValue: '2', checked: true },
                    { boxLabel: '庫備品', name: 'rb2', width: 70, inputValue: '1' },
                    { boxLabel: '非庫備品', name: 'rb2', width: 70, inputValue: '0', }
                ]
            }]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [{
                xtype: 'radiogroup',
                fieldLabel: '是否核撥',
                width: 230,
                labelWidth: 60,
                items: [
                    { boxLabel: '已核撥', name: 'rb3', width: 70, inputValue: '1', checked: true },
                    { boxLabel: '未核撥(不含申請中)', name: 'rb3', width: 70, inputValue: '2' }
                ]
            }, {
                xtype: 'radiogroup',
                fieldLabel: '申請單分類',
                width: 300,
                labelWidth: 80,
                items: [
                    { boxLabel: '不區分', name: 'rb4', width: 70, inputValue: '0', checked: true },
                    { boxLabel: '常態申請', name: 'rb4', width: 70, inputValue: '1' },
                    { boxLabel: '臨時申請', name: 'rb4', width: 70, inputValue: '2' }
                ]
            }, {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    var f = T1Query.getForm();
                    if (f.isValid()) {
                        P0 = f.findField('P0').getValue();
                        P1 = f.findField('P1').rawValue;
                        P2 = f.findField('P2').rawValue;
                        P3 = f.findField('rb1').getGroupValue();
                        P4 = f.findField('rb2').getGroupValue();
                        P5 = f.findField('rb3').getGroupValue();
                        P6 = f.findField('rb4').getGroupValue();
                        P7 = f.findField('P0').rawValue;

                        T1Load();

                        var column = T1Grid.getColumns();
                        if (P3 == '1') {
                            for (var i = 1; i < 8; i++) {
                                column[i].show();
                            }
                            for (var i = 8; i < 14; i++) {
                                column[i].hide();
                            }
                        }
                        else {
                            for (var i = 1; i < 8; i++) {
                                column[i].hide();
                            }
                            for (var i = 8; i < 14; i++) {
                                column[i].show();
                            }
                        }
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
                    f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                }
            }]
        }
        ]
    });

    var T1Store = Ext.create('WEBAPP.store.FA0076', { // 定義於/Scripts/app/store/AA/AA0092.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    P0: P0,
                    P1: P1,
                    P2: P2,
                    P3: P3,
                    P4: P4,
                    P5: P5,
                    P6: P6,
                    P7: P7
                };
                Ext.apply(store.proxy.extraParams, np);

            },
        }
    });



    function T1Load() {

        T1Store.load({
            params: {
                start: 0
            }
        });
    }
    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'P0', value: P0 }); //SQL篩選條件
                            p.push({ name: 'P1', value: P1 }); //SQL篩選條件
                            p.push({ name: 'P2', value: P2 }); //SQL篩選條件
                            p.push({ name: 'P3', value: P3 }); //SQL篩選條件
                            p.push({ name: 'P4', value: P4 }); //SQL篩選條件
                            p.push({ name: 'P5', value: P5 }); //SQL篩選條件
                            p.push({ name: 'P6', value: P6 }); //SQL篩選條件
                            p.push({ name: 'P7', value: P7 }); //SQL篩選條件
                            PostForm(T1GetExcel, p);

                        }
                    });
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
            }, {
                text: "成本碼",
                dataIndex: 'APPDEPT',
                width: 70
            }, {
                text: "單位名稱",
                dataIndex: 'APPDEPTNAME',
                width: 120
            }, {
                text: "申請日期",
                dataIndex: 'APPDATE',
                width: 70
            }, {
                text: "申請張數",
                dataIndex: 'DOCNO_CNT',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "申請項數",
                dataIndex: 'MMCODE_CNT',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "申請金額",
                dataIndex: 'AMT_SUM',
                style: 'text-align:left',
                align: 'right',
                width: 150
            }, {
                text: "撥發金額",
                dataIndex: 'APV_SUM',
                style: 'text-align:left',
                align: 'right',
                width: 150
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200
            }, {
                text: "合約單價",
                dataIndex: 'M_CONTPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 150
            }, {
                text: "申請總量",
                dataIndex: 'APP_SUM',
                style: 'text-align:left',
                align: 'right',
                width: 150
            }, {
                text: "核撥總量",
                dataIndex: 'APV_SUM',
                style: 'text-align:left',
                align: 'right',
                width: 150
            }, {
                header: "",
                flex: 1
            }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
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

    var column = T1Grid.getColumns();

    for (var i = 1; i < 8; i++) {
        column[i].show();
    }
    for (var i = 8; i < 14; i++) {
        column[i].hide();
    }
});


