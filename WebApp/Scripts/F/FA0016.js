Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

var MATComboGet = '../../../api/FA0016/GetMATCombo';
var matUserID;

// 物品類別清單
var MATQueryStore = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT']
});


Ext.getUrlParam = function (param) {
    var params = Ext.urlDecode(location.search.substring(1));
    return param ? params[param] : params;
};
function setMatClass() {
    matClass = Ext.getUrlParam('matcls');
    if (matClass == "userid") {  // AA0070物料分類需取得使用者帳號        
        matUserID = true;
    } else {
        matUserID = false;
    }
}
setMatClass();

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
                MATQueryStore.add({ VALUE: '38', TEXT: '一般物品類', COMBITEM: '一般物品類' });
            }
        },
        failure: function (response, options) {

        }
    });
}
setComboData();

var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
    name: 'P3',
    fieldLabel: '院內碼',
    //fieldCls: 'required',
    //allowBlank: false,
    //width: 150,
    padding: '0 4 0 0',
    //限制一次最多顯示10筆
    limit: 10,

    //指定查詢的Controller路徑
    queryUrl: '/api/FA0016/GetMMCodeCombo',

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

var T1GetExcel = '/api/FA0016/Excel';
var reportUrl = '/Report/F/FA0016.aspx';

var P0 = '';
var P1 = '';
var P2 = '';
var P3 = '';
var P4 = '';

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
            id: 'PanelP1',
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
                    fieldLabel: '核撥日期',
                    name: 'P1',
                    labelWidth: 80,
                    width: 160,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    allowBlank: false,
                    value: new Date()
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
                    value: new Date()
                }, mmCodeCombo,
                {
                    xtype: 'container',
                    defaultType: 'checkboxfield',
                    layout: 'hbox',
                    items: [
                        {
                            boxLabel: '狀態為核撥確認',
                            name: 'flowid6only',
                            inputValue: 'Y',
                            id: 'flowid6only',
                            width: 120,
                            padding: '0 0 0 20'
                        }
                    ]
                },
                {
                    xtype: 'container',
                    defaultType: 'checkboxfield',
                    layout: 'hbox',
                    items: [
                        {
                            boxLabel: '核撥量>0',
                            name: 'apvqtynot0',
                            inputValue: 'Y',
                            id: 'apvqtynot0',
                            width: 100
                        }
                    ]
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = T1Query.getForm();
                        if (f.isValid()) {
                            P0 = f.findField('P0').getValue();
                            P1 = f.findField('P1').rawValue;
                            P2 = f.findField('P2').rawValue;
                            P3 = f.findField('P3').getValue();
                            P4 = f.findField('P0').rawValue;
                            T1Load();
                        }
                        else {
                            Ext.MessageBox.alert('提示','請輸入必填欄位');
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
                }


            ]
        },
            
        ]
    });

    var T1Store = Ext.create('WEBAPP.store.FA0016', { // 定義於/Scripts/app/store/AA/AA0092.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    P0: P0,
                    P1: P1,
                    P2: P2,
                    P3:P3
                };
                Ext.apply(store.proxy.extraParams, np);
                
                T1Store.getProxy().setExtraParam('flowid6only', Ext.getCmp('flowid6only').getValue() == true ? 'Y' : 'N');
                T1Store.getProxy().setExtraParam('apvqtynot0', Ext.getCmp('apvqtynot0').getValue() == true ? 'Y' : 'N');

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
                            p.push({ name: 'flowid6only', value: Ext.getCmp('flowid6only').getValue() == true ? 'Y' : 'N' }); //SQL篩選條件
                            p.push({ name: 'apvqtynot0', value: Ext.getCmp('apvqtynot0').getValue() == true ? 'Y' : 'N' }); //SQL篩選條件
                            PostForm(T1GetExcel, p);

                        }
                    });
                }
            },
            {
                id: 't1print', text: '列印', disabled: false, handler: function () {
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
                text: "責任中心",
                dataIndex: 'APPDEPT',
                width: 60
            }, {
                text: "單位名稱",
                dataIndex: 'APPDEPTNAME',
                width: 100
            },{
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 70
            }, {
                text: "物料分類",
                dataIndex: 'MAT_CLASS',
                width: 70
            }, {
                text: "中文品名",
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
                text: "核撥日期",
                dataIndex: 'APVTIME',
                width: 120
            }, {
                text: "數量",
                dataIndex: 'APVQTY',
                style: 'text-align:left',
                align: 'right',
                width: 60
            }, {
                text: "庫存平均單價",
                dataIndex: 'M_CONTPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 120
            },
            {
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 160
            }, {
                text: "申請單狀態",
                dataIndex: 'FLOWID',
                width: 120
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
                html: '<iframe src="' + reportUrl + '?MAT_CLASS=' + P0 + '&APVTIME_B=' + P1 + '&APVTIME_E=' + P2 + '&MMCODE=' + P3 + '&MAT_CLASS_N=' + P4
                    + '&flowid6only=' + (Ext.getCmp('flowid6only').getValue() == true ? 'Y' : 'N')
                    + '&apvqtynot0=' + (Ext.getCmp('apvqtynot0').getValue() == true ? 'Y' : 'N')
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
