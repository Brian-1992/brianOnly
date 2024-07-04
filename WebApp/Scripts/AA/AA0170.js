Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

function SetDate() {
    nowDate = new Date();
    nowDate.getMonth();
    nowDate = Ext.Date.format(nowDate, "Ymd") - 19110000;
    nowDate = nowDate.toString().substring(0, 5);

    T1Query.getForm().findField('P1').setValue(nowDate);

}

var MATComboGet = '../../../api/FA0035/GetMATCombo';
var matUserID;
Ext.override(Ext.grid.column.Column, { menuDisabled: true });
var WH_NOComboGet = '../../../api/AA0170/GetWH_NOComboOne';  //庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫)

// 庫房代碼
var wh_noStore = Ext.create('Ext.data.Store', {
    fields: ['WH_NO', 'WH_NAME']
});

// 物品類別清單
var MATQueryStore = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT'],
    data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
        { "VALUE": "01", "TEXT": "01-藥品" },
        { "VALUE": "02", "TEXT": "02-衛材(含檢材)" },
        { "VALUE": "07", "TEXT": "07-被服" },
        { "VALUE": "08", "TEXT": "08-資訊耗材" },
        { "VALUE": "0X", "TEXT": "0X-一般行政類" }
    ]
});

// 藥品類別清單
var type0Store = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT'],
    data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
        { "VALUE": "99", "TEXT": "全部" },
        { "VALUE": "1", "TEXT": "口服(藥庫)" },
        { "VALUE": "2", "TEXT": "非口服(藥庫)" },
        { "VALUE": "3", "TEXT": "1~3級管制" },
        { "VALUE": "4", "TEXT": "4級管制" },
        { "VALUE": "5", "TEXT": "公藥(麻醉科)" },
        { "VALUE": "6", "TEXT": "專科(麻醉科)" },
        { "VALUE": "7", "TEXT": "一般藥品(一般庫房)" },
        { "VALUE": "8", "TEXT": "大瓶點滴(一般庫房)" },
        { "VALUE": "X", "TEXT": "不區分(藥局)" }
    ]
});

// 衛材類別清單
var type1Store = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT'],
    data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
        { "VALUE": "99", "TEXT": "全部" },
        { "VALUE": "1", "TEXT": "庫備品" },
        { "VALUE": "0", "TEXT": "非庫備品" },
        { "VALUE": "3", "TEXT": "小額採購" },
    ]
});




function setComboData() {
    Ext.Ajax.request({
        url: WH_NOComboGet,
        params: { limit: 10, page: 1, start: 0 },

        method: reqVal_p,
        success: function (response) {
            var data = Ext.decode(response.responseText);

            if (data.success) {
                var tb_wh_no = data.etts;
                if (tb_wh_no.length > 0) {
                    for (var i = 0; i < tb_wh_no.length; i++) {
                        wh_noStore.add({ WH_NO: tb_wh_no[i].WH_NO, WH_NAME: tb_wh_no[i].WH_NAME });

                    }
                }
            }
        },
        failure: function (response, options) {

        }
    });
}
setComboData();


var T1GetExcel = '/api/AA0170/Excel';
var reportUrl = '/Report/A/AA0170.aspx';
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
                    fieldLabel: '庫房',
                    xtype: 'combo',
                    store: wh_noStore,
                    name: 'P1',
                    id: 'P1',
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    displayField: 'WH_NO',
                    valueField: 'WH_NO',
                    requiredFields: ['WH_NAME'],
                    tpl: new Ext.XTemplate(
                        '<tpl for=".">',
                        '<tpl if="VALUE==\'\'">',
                        '<div class="x-boundlist-item" style="height:auto;">{WH_NO}&nbsp;</div>',
                        '<tpl else>',
                        '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                        '<span style="color:red">{WH_NO}</span><br/>&nbsp;<span style="color:blue">{WH_NAME}</span></div>',
                        '</tpl></tpl>', {
                            formatText: function (text) {
                                return Ext.util.Format.htmlEncode(text);
                            }
                        }),
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {

                        }
                    },
                    padding: '0 4 0 4'
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '月份別',
                    name: 'P2',
                    id: 'P2',
                    width: 160,
                    labelWidth: 60,
                    fieldCls: 'required',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    padding: '0 4 0 4',
                    allowBlank: false
                }
            ]
        },
        {

            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {

                    xtype: 'combo',
                    fieldLabel: '物料類別',
                    name: 'P0',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 180,
                    padding: '0 4 0 4',
                    store: MATQueryStore,
                    fieldCls: 'required',
                    allowBlank: false,
                    value: '02',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    listeners:
                    {
                        change: function (rg, nVal, oVal, eOpts) {
                            console.log(nVal);
                            showkind(nVal)
                        }
                    }
                }, {
                    xtype: 'combo',
                    fieldLabel: '庫備類別',
                    name: 'rb1',
                    id: 'rb1',
                    enforceMaxLength: true,
                    hidden: true,
                    labelWidth: 60,
                    width: 200,
                    padding: '0 4 0 4',
                    store: type0Store,
                    fieldCls: 'required',
                    allowBlank: false,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    value: '99',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                }, {

                    xtype: 'combo',
                    fieldLabel: '庫備類別',
                    name: 'rb2',
                    id: 'rb2',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 200,
                    padding: '0 4 0 4',
                    store: type1Store,
                    fieldCls: 'required',
                    allowBlank: false,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    value: '99',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = T1Query.getForm();
                        if (f.isValid()) {
                            P0 = f.findField('P0').getValue();
                            P1 = f.findField('P1').getValue();
                            P2 = f.findField('P2').rawValue;
                            P3 = f.findField('rb1').getValue();
                            P4 = f.findField('rb2').getValue();

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
                }

            ]
        }]
    });
    function showkind(value) {
        console.log(value)
        var f = T1Query.getForm();
        f.findField('rb1').setValue('99');
        f.findField('rb2').setValue('99');
        if (value == 1) {
            f.findField("rb1").setVisible(true);
            f.findField("rb2").setVisible(false);
        }
        else {
            f.findField("rb2").setVisible(true);
            f.findField("rb1").setVisible(false);

        }
    }
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['WH_NO', 'WH_NAME', 'MMCODE', 'MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'STORE_QTY', 'CHK_QTY', 'USE_QTY', 'STORE_AMOUNT', 'CHKAMOUNT']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });


    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'CHK_WH_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    P0: P0,
                    P1: P1,
                    P2: P2,
                    P3: P3,
                    P4: P4
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
            url: '/api/AA0170/GetAll',
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
    }
    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't1print', text: '列印', disabled: false, hidden: true,
                handler: function () {
                    Ext.MessageBox.confirm('列印', '是否確定列印？', function (btn, text) {
                        if (btn === 'yes') {
                            showReport();
                        }
                    });
                }
            }, {
                itemId: 'export', text: '匯出', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            var fn = getFileName();
                            
                            p.push({ name: 'P0', value: P0 }); //SQL篩選條件
                            p.push({ name: 'P1', value: P1 }); //SQL篩選條件
                            p.push({ name: 'P2', value: P2 }); //SQL篩選條件
                            p.push({ name: 'rb1', value: T1Query.getForm().findField('rb1').getValue() }); //SQL篩選條件
                            p.push({ name: 'rb2', value: T1Query.getForm().findField('rb2').getValue() }); //SQL篩選條件
                            p.push({ name: 'fn', value: fn });

                            PostForm(T1GetExcel, p);

                        }
                    });
                }
            }
        ]
    });

    function getFileName() {
        var fn = '';
        var mat_class = T1Query.getForm().findField('P0').rawValue;
        var wh_no = T1Query.getForm().findField('P1').getValue();
        var ym = T1Query.getForm().findField('P2').rawValue;
        
        var rb1 = T1Query.getForm().findField('rb1').rawValue;
        var rb2 = T1Query.getForm().findField('rb2').rawValue;

        fn += (ym + '_' + mat_class + '_');
        if (wh_no != '' && wh_no != null) {
            fn += (wh_no + '_');
        }
        if (T1Query.getForm().findField('P0').getValue() == '01') {
            fn += (rb1 + '_');
        } else {
            fn += (rb2 + '_');
        }
        fn += '盤點情況表.xlsx';
        return fn;
    }

    function showReport() {
        if (!win) {
            var np = {
                p0: T1Query.getForm().findField('P0').getValue(),
                p1: T1Query.getForm().findField('P1').rawValue,
                p2: T1Query.getForm().findField('P2').rawValue,
                p3: T1Query.getForm().findField('rb1').getGroupValue(),
                p4: T1Query.getForm().findField('rb2').getGroupValue(),

            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '&p4=' + np.p4 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
                text: "庫房代碼",
                dataIndex: 'CHK_WH_NO',
                width: 70
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                width: 70
            }, {
                text: "庫房盤點階段",
                dataIndex: 'MAX_CHK_LEVEL_NAME',
                width: 90
            }, {
                text: "庫房盤點狀態",
                dataIndex: 'FINAL_STATUS_NAME',
                width: 90
            }
            , {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 70
            }, {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 150
            }, {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 150
            }, {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                style: 'text-align:left',
                align: 'left',
                width: 50
            }, {
                text: "優惠合約單價",
                dataIndex: 'DISC_CPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "初盤電腦量",
                dataIndex: 'STORE_QTY1',
                style: 'text-align:left',
                align: 'right',
                width: 90
            }, {
                text: "初盤盤點量",
                dataIndex: 'CHK_QTY1',
                style: 'text-align:left',
                align: 'right',
                width: 90
            }, {
                text: "複盤電腦量",
                dataIndex: 'STORE_QTY2',
                style: 'text-align:left',
                align: 'right',
                width: 90
            }, {
                text: "複盤盤點量",
                dataIndex: 'CHK_QTY2',
                style: 'text-align:left',
                align: 'right',
                width: 90
            }, {
                text: "三盤電腦量",
                dataIndex: 'STORE_QTY3',
                style: 'text-align:left',
                align: 'right',
                width: 90
            }, {
                text: "三盤盤點量",
                dataIndex: 'CHK_QTY3',
                style: 'text-align:left',
                align: 'right',
                width: 90
            }, {
                text: "初盤單號",
                dataIndex: 'CHK_NO1',
                style: 'text-align:left',
                align: 'right',
                width: 130
            }, {
                text: "複盤單號",
                dataIndex: 'CHK_NO2',
                style: 'text-align:left',
                align: 'right',
                width: 130
            }, {
                text: "三盤單號",
                dataIndex: 'CHK_NO3',
                style: 'text-align:left',
                align: 'right',
                width: 130
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

});
