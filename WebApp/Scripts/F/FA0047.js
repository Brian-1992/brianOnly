Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

var MATComboGet = '../../../api/FA0047/GetMATCombo';
var matUserID;
var today = new Date();
var y = today.getFullYear();
var m = today.getMonth();
var d = today.getDate();

var date_b = new Date(y,m-1);
var date_e = new Date(y, m-1);


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

var T1GetExcel = '/api/FA0047/Excel';
var reportUrl = '/Report/F/FA0047.aspx';

var P0 = '';
var P1 = '';
var P2 = '';
var P3 = '';
var P4 = '';
var P5 = '';
var P6 = '';

var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
    name: 'MMCODE',
    fieldLabel: '院內碼',
    padding: '0 10 0 4',
    //width: 150,

    //限制一次最多顯示10筆
    limit: 10,

    //指定查詢的Controller路徑
    queryUrl: '/api/FA0047/GetMMCodeCombo',

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
                {
                    xtype: 'combo',
                    fieldLabel: '物料類別',
                    name: 'P0',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 170,
                    padding: '0 4 4 4',
                    store: MATQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'monthfield',
                    fieldLabel: '年月',
                    name: 'P1',
                    labelWidth: mLabelWidth,
                    width: 150,
                    padding: '0 4 4 4',
                    fieldCls: 'required',
                    allowBlank: false,
                    value: date_b
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '至',
                    name: 'P2',
                    labelWidth: 10,
                    width: 88,
                    padding: '0 4 4 4',
                    labelSeparator: '',
                    fieldCls: 'required',
                    allowBlank: false,
                    value: date_e
                }, {
                    xtype: 'radiogroup',
                    fieldLabel: '軍民別',
                    width: 150,
                    labelWidth: 60,
                    items: [
                        { boxLabel: '軍', name: 'rb1', width: 40, readOnly: true, inputValue: '1' },
                        { boxLabel: '民', name: 'rb1', width: 40, readOnly: true, inputValue: '2', checked: true}
                    ]
                }
            ]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [{
                xtype: 'radiogroup',
                fieldLabel: '庫備類別',
                width: 300,
                labelWidth: 60,
                items: [
                    { boxLabel: '所有品項', name: 'rb2', width: 70, inputValue: '2', checked: true  },
                    { boxLabel: '庫備品', name: 'rb2', width: 70, inputValue: '1'},
                    { boxLabel: '非庫備品', name: 'rb2', width: 70, inputValue: '0', }
                ]
            }, mmCodeCombo ,{
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
                            P5 = f.findField('MMCODE').getValue();
                            P6 = f.findField('P0').rawValue;

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

    var T1Store = Ext.create('WEBAPP.store.FA0047', { // 定義於/Scripts/app/store/AA/AA0092.js
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
                            p.push({ name: 'P2', value: P2}); //SQL篩選條件
                            p.push({ name: 'P3', value: P3 }); //SQL篩選條件
                            p.push({ name: 'P4', value: P4 }); //SQL篩選條件
                            p.push({ name: 'P5', value: P5 }); //SQL篩選條件
                            p.push({ name: 'P6', value: P6 }); //SQL篩選條件
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
                text: "軍民別",
                dataIndex: 'MILTYPE',
                width: 60
            }, {
                text: "中文品名 ",
                dataIndex: 'MMNAME_C',
                width: 200
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200
            }, {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 60
            }, {
                text: "年月",
                dataIndex: 'DATA_YM',
                width: 60
            }, {
                text: "責任中心",
                dataIndex: 'INID',
                width: 60
            }, {
                text: "是否庫備",
                dataIndex: 'M_STOREID',
                width: 60
            }, {
                text: "是否合約",
                dataIndex: 'M_CONTID',
                width: 60
            }, {
                text: "期初數量",
                dataIndex: 'P_INV_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 70
            }, {
                text: "期初單價",
                dataIndex: 'PMN_AVGPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 70
            }, {
                text: "期初金額",
                dataIndex: 'PMN_AMT',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "進貨數量",
                dataIndex: 'IN_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "進貨單價",
                dataIndex: 'IN_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "進貨金額",
                dataIndex: 'IN_AMT',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "撥發數量",
                dataIndex: 'OUT_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "撥發單價",
                dataIndex: 'AVG_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "撥發金額",
                dataIndex: 'OUT_AMT',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "結存數量",
                dataIndex: 'INV_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "庫存單價",
                dataIndex: 'INV_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "結存金額",
                dataIndex: 'INV_AMT',
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
                + '&P5=' + P5
                + '&P6=' + P6
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


