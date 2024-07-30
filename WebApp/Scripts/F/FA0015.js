Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

var MATComboGet = '../../../api/FA0015/GetMATCombo';
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
    if (matClass == "userid") {
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
                var mat_cls = data.etts;
                if (mat_cls.length > 0) {
                    MATQueryStore.add({ VALUE: '', TEXT: '' });
                    for (var i = 0; i < mat_cls.length; i++) {
                        MATQueryStore.add({ VALUE: mat_cls[i].VALUE, TEXT: mat_cls[i].TEXT });
                    }
                }
            }
        },
        failure: function (response, options) {

        }
    });
}
setComboData();

var T1GetExcel = '/api/FA0015/Excel';
var reportUrl = '/Report/F/FA0015.aspx';

var P0 = '';
var P1 = '';
var P2 = '';
//var P3 = '';


Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "戰備調撥統計報表";

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
                //{
                //    xtype: 'combo',
                //    fieldLabel: '物料類別',
                //    name: 'P0',
                //    enforceMaxLength: true,
                //    labelWidth: 60,
                //    width: 170,
                //    padding: '0 4 0 4',
                //    store: MATQueryStore,
                //    displayField: 'TEXT',
                //    valueField: 'VALUE',
                //    queryMode: 'local',
                //    anyMatch: true,
                //    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                //},
                {
                    xtype: 'displayfield',
                    fieldLabel: '物料類別',
                    value:'02 衛材'
                },
                {
                    xtype: 'monthfield',
                    fieldLabel: '核撥年月',
                    name: 'P1',
                    labelWidth: 100,
                    width: 180,
                    fieldCls: 'required',
                    allowBlank: false,
                    editable: false
                },
                {
                    xtype: 'monthfield',
                    fieldLabel: '至',
                    name: 'P2',
                    labelWidth: 20,
                    width: 100,
                    padding: '0 50 0 0',
                    labelSeparator: '',
                    fieldCls: 'required',
                    allowBlank: false,
                    editable: false
                },
                //{
                //    xtype: 'fieldcontainer',
                //    fieldLabel: '僅顯示未完成歸墊',
                //    defaultType: 'checkboxfield',
                //    labelWidth: 110,
                //    labelSeparator: '',
                //    width: 150,
                //    items: [
                //        {
                //            name: 'P3',
                //            inputValue: '1',
                //        }
                //    ]
                //},
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = T1Query.getForm();
                        P0 = '02';
                        P1 = f.findField('P1').rawValue;
                        P2 = f.findField('P2').rawValue;
                        //P3 = f.findField('P3').checked;
                        if (T1Query.getForm().findField('P1').isValid() && T1Query.getForm().findField('P2').isValid()) {
                            T1Load();
                        }
                        else {
                            Ext.Msg.alert('訊息', '<span style="color:red; font-weight:bold">核撥年月</span>資料不正確。');
                            msglabel('訊息區:核撥年月資料不正確。');
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('WH_NO').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }


            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.FA0015VM', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    P0: P0,
                    P1: P1,
                    P2: P2,
                    //P3: P3,
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
                            //p.push({ name: 'P3', value: P3 }); //SQL篩選條件
                            //p.push({ name: 'P3', value: T1Query.getForm().findField('P1').rawValue }); //SQL篩選條件
                            //p.push({ name: 'P4', value: T1Query.getForm().findField('P2').rawValue }); //SQL篩選條件
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
            },
            //{
            //    text: "申請單號",
            //    dataIndex: 'DOCNO',
            //    width: 160
            //},
            //{
            //    text: "庫房代碼",
            //    dataIndex: 'WH_NO',
            //    width: 60
            //},
            //{
            //    text: "庫房名稱",
            //    dataIndex: 'WH_NAME',
            //    width: 100
            //},
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            },
            //{
            //    text: "中文品名",
            //    dataIndex: 'MMNAME_C',
            //    width: 200
            //},
            //{
            //    text: "英文品名",
            //    dataIndex: 'MMNAME_E',
            //    width: 200
            //},
            //{
            //    text: "計量單位",
            //    dataIndex: 'BASE_UNIT',
            //    width: 60
            //},
            {
                text: "調撥日期",
                dataIndex: 'DIS_DATEYM',
                width: 80
            },
            {
                text: "數量",
                dataIndex: 'BW_MQTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            },
            //{
            //    text: "歸墊量",
            //    dataIndex: 'RV_MQTY',
            //    style: 'text-align:left',
            //    align: 'right',
            //    width: 60
            //},
            //{
            //    text: "申請日",
            //    dataIndex: 'APPTIME',
            //    width: 60
            //},
            //{
            //    text: "調撥日",
            //    dataIndex: 'DIS_TIME',
            //    width: 60
            //},
            {
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
                    + '?MAT_CLASS=' + P0
                    + '&DIS_TIME_B=' + P1
                    + '&DIS_TIME_E=' + P2
                    //+ '&P3=' + P3
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

    T1Query.getForm().findField('P1').setValue(new Date(y, m));
    T1Query.getForm().findField('P2').setValue(new Date(y, m));

});
