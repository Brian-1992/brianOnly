Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

var MATComboGet = '../../../api/FA0046/GetMATCombo';
var matUserID;



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

var T1GetExcel = '/api/FA0046/Excel';
var reportUrl = '/Report/F/FA0046.aspx';

var P0 = '';
var P1 = '';
var P2 = '';
var P3 = '';
var P4 = '';
var P5 = '';
var P6 = '';

var wh_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
    name: 'WH_NO',
    fieldLabel: '庫房代碼',
    fieldCls: 'required',
    allowBlank: false,
    //width: 150,

    //限制一次最多顯示10筆
    limit: 10,

    //指定查詢的Controller路徑
    queryUrl: '/api/AA0059/GetWH_NoCombo',

    //查詢完會回傳的欄位
    extraFields: ['MAT_CLASS', 'BASE_UNIT'],

    //查詢時Controller固定會收到的參數
    getDefaultParams: function () {
        //var f = T2Form.getForm();
        //if (!f.findField("MMCODE").readOnly) {
        //    tmpArray = f.findField("FRWH2").getValue().split(' ');
        //    return {
        //        //MMCODE: f.findField("MMCODE").getValue(),
        //        WH_NO: tmpArray[0]
        //    };
        //}
    },
    listeners: {
        select: function (c, r, i, e) {
            //選取下拉項目時，顯示回傳值
            //alert(r.get('MAT_CLASS'));
            //var f = T2Form.getForm();
            //if (r.get('MMCODE') !== '') {
            //    f.findField("MMCODE").setValue(r.get('MMCODE'));
            //    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
            //    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
            //    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
            //}
        }
    }
});

var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
    name: 'MMCODE',
    fieldLabel: '院內碼',
    padding: '0 10 0 4',
    //width: 150,

    //限制一次最多顯示10筆
    limit: 10,

    //指定查詢的Controller路徑
    queryUrl: '/api/FA0046/GetMMCodeCombo',

    //查詢完會回傳的欄位
    extraFields: ['MAT_CLASS', 'BASE_UNIT'],

    //查詢時Controller固定會收到的參數
    getDefaultParams: function () {
        //var f = T2Form.getForm();
        //if (!f.findField("MMCODE").readOnly) {
        //    tmpArray = f.findField("FRWH2").getValue().split(' ');
        //    return {
        //        //MMCODE: f.findField("MMCODE").getValue(),
        //        WH_NO: tmpArray[0]
        //    };
        //}
    },
    listeners: {
        select: function (c, r, i, e) {
            //選取下拉項目時，顯示回傳值
            //alert(r.get('MAT_CLASS'));
            //var f = T2Form.getForm();
            //if (r.get('MMCODE') !== '') {
            //    f.findField("MMCODE").setValue(r.get('MMCODE'));
            //    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
            //    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
            //    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
            //}
        }
    }
});


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
                wh_NoCombo,
                {
                    xtype: 'monthfield',
                    fieldLabel: '年月',
                    name: 'P1',
                    labelWidth: mLabelWidth,
                    width: 150,
                    padding: '0 4 4 4',
                    fieldCls: 'required',
                    allowBlank: false,
                    value: new Date()
                }
            ]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [{
                xtype: 'combobox',
                fieldLabel: '類別',
                name: 'P2',
                id: 'P2',
                queryMode: 'local',
                allowBlank: false,
                fieldCls: 'required',
                displayField: 'name',
                valueField: 'abbr',
                width: 200,
                editable: false,
                store: [
                    { abbr: '01', name: '01-藥品' },
                    { abbr: '02', name: '02-衛材(含檢材)' },       
                    { abbr: '07', name: '07-被服' },
                    { abbr: '08', name: '08-資訊耗材' },
                    { abbr: '0X', name: '0X-一般行政類' },
                ],
                value: '02',
                padding: '0 20 0 4',
                listeners: {
                    change: function (_this, newValue, oldValue, eOpts) {
                        
                        if (newValue == '01') {
                            T1Query.getForm().findField('rg1').hide();
                            T1Query.getForm().findField('rg2').show();
                        }
                        else {
                            T1Query.getForm().findField('rg2').hide();
                            T1Query.getForm().findField('rg1').show();
                        }

                    }
                }
            }, {
                xtype: 'radiogroup',
                fieldLabel: '',
                name: 'rg1',
                width: 170,
                labelWidth: 60,
                items: [
                    { boxLabel: '庫備品', name: 'rb1', width: 70, inputValue: '1', checked: true },
                    { boxLabel: '非庫備品', name: 'rb1', width: 70, inputValue: '0', }
                ],
                padding: '0 4 0 4',
            }, {
                xtype: 'radiogroup',
                fieldLabel: '',
                name: 'rg2',
                width: 370,
                labelWidth: 60,
                items: [
                    { boxLabel: '口服', name: 'rb2', width: 70, inputValue: '1', checked: true },
                    { boxLabel: '非口服', name: 'rb2', width: 70, inputValue: '2' },
                    { boxLabel: '1~3級管制', name: 'rb2', width: 100, inputValue: '3', },
                    { boxLabel: '4級管制', name: 'rb2', width: 70, inputValue: '4', },
                    { boxLabel: '不區分', name: 'rb2', width: 70, inputValue: 'X', }
                ],
                padding: '0 4 0 4',
                hidden: true
            }, {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    var f = T1Query.getForm();
                    if (f.isValid()) {
                        P0 = f.findField('WH_NO').getValue();
                        P1 = f.findField('P1').rawValue;
                        P2 = f.findField('P2').getValue();
                        P3 = f.findField('rb1').getGroupValue();
                        P4 = f.findField('rb2').getGroupValue();

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
                    f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                }
            }]
        }
        ]
    });

    var T1Store = Ext.create('WEBAPP.store.FA0046', { // 定義於/Scripts/app/store/AA/AA0092.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    P0: P0,
                    P1: P1,
                    P2: P2,
                    P3: P3,
                    P4: P4,
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
                            PostForm(T1GetExcel, p);

                        }
                    });
                }
            },
            {
                itemId: 't1print', text: '列印', disabled: false, handler: function () {
                    showReport();
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
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 70
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 60
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200
            }, {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                width: 60
            }, {
                text: "上期結存單價",
                dataIndex: 'PMN_AVGPRICE',
                width: 60
            }, {
                text: "上月結存",
                dataIndex: 'PMN_INVQTY',
                width: 60
            }, {
                text: "上月金額",
                dataIndex: 'PMN_AMOUNT',
                width: 60
            }, {
                text: "合約單價",
                dataIndex: 'CONT_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 70
            }, {
                text: "本月進貨",
                dataIndex: 'MN_INQTY',
                style: 'text-align:left',
                align: 'right',
                width: 70
            }, {
                text: "進貨金額",
                dataIndex: 'IN_AMOUNT',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "庫存成本單價",
                dataIndex: 'AVG_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "本月消耗",
                dataIndex: 'USE_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "消耗金額",
                dataIndex: 'USE_AMOUNT',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "期未單價",
                dataIndex: 'AVG_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "本月結存",
                dataIndex: 'STORE_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "結存金額",
                dataIndex: 'STORE_AMOUNT',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "本月盤盈虧",
                dataIndex: 'DIFF_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "盤盈虧金額",
                dataIndex: 'DIFF_AMOUNT',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "周轉率",
                dataIndex: 'TURNOVER',
                style: 'text-align:left',
                align: 'right',
                width: 100
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
                html: '<iframe src="' + reportUrl
                + '?P0=' + P0
                + '&P1=' + P1
                + '&P2=' + P2
                + '&P3=' + P3
                + '&P4=' + P4
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


});


