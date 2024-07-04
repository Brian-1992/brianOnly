Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var St_WhGet = '../../../api/BD0021/GetWhCombo';
    var St_MatclassGet = '../../../api/BD0021/GetMatclassCombo';
    var T1GetExcel = '../../../api/BD0021/Excel';
    var reportUrl = '/Report/B/BD0021.aspx';

    var T1LastRec = null;

    // 庫房
    var st_wh = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    // 物料分類
    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P2',
        name: 'P2',
        fieldLabel: '院內碼',
        allowBlank: true,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/BD0021/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            var f = T1Query.getForm();
            if (!f.findField("P1").readOnly) {
                var mat_class = f.findField("P1").getValue();
                return {
                    MAT_CLASS: mat_class
                };
            }
        },
        listeners: {
        }
    });

    //合約
    var contStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "", "TEXT": "全部" },
            { "VALUE": "0", "TEXT": "0 合約" },
            { "VALUE": "2", "TEXT": "2 非合約" }
        ]
    });

    var T11QueryAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
        name: 'P5',
        id: 'P5',
        fieldLabel: '廠商代碼',
        limit: 20,
        queryUrl: '/api/BD0021/GetAgennoCombo',
        width: 150,
        matchFieldWidth: false,
        listConfig: { width: 300 }
    });

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 200;
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
            xtype: 'container',
            layout: 'vbox',
            items: [{
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'textfield',
                    fieldLabel: '訂單編號',
                    id: 'P4',
                    name: 'P4',
                    width: 200
                }, {
                    xtype: 'combo',
                    fieldLabel: '庫房',
                    id: 'P0',
                    name: 'P0',
                    store: st_wh,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    multiSelect: false,
                    labelWidth: 100,
                    width: 308,
                    listeners: {
                        select: function (ele, newValue, oldValue) {
                            setComboMatClass();
                        }
                    }
                }, {
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    id: 'P1',
                    name: 'P1',
                    store: st_matclass,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    multiSelect: false,
                    width: 200
                }, {
                    xtype: 'combo',
                    fieldLabel: '是否合約',
                    id: 'P3',
                    name: 'P3',
                    store: contStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    multiSelect: false,
                    value: '',
                    width: 200
                }]
            }, {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'container',
                    layout: 'hbox',
                    items: [
                        mmCodeCombo
                    ]
                }, {
                    xtype: 'datefield',
                    fieldLabel: '訂單日期區間',
                    id: 'D0',
                    name: 'D0',
                    labelWidth: 100,
                    width: 200
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    id: 'D1',
                    name: 'D1',
                    labelWidth: 8,
                    width: 108
                },
                    T11QueryAgenno,
                {
                    xtype: 'tbspacer',
                    width: 10
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        T1Query.getForm().findField('P0').reset();
                        T1Query.getForm().findField('P1').reset();
                        T1Query.getForm().findField('P2').reset();
                        T1Query.getForm().findField('P3').reset();
                        T1Query.getForm().findField('P4').reset();
                        T1Query.getForm().findField('D0').reset();
                        T1Query.getForm().findField('D1').reset();

                        var f = this.up('form').getForm();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在D0
                        msglabel('訊息區:');
                    }
                }]
            }]
        }]
    });

    function setComboWhNo() {
        st_wh.removeAll();
        //庫房
        Ext.Ajax.request({
            url: St_WhGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_wh = data.etts;
                    if (tb_wh.length > 0) {
                        for (var i = 0; i < tb_wh.length; i++) {
                            st_wh.add({ VALUE: tb_wh[i].VALUE, COMBITEM: tb_wh[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function setComboMatClass() {
        st_matclass.removeAll();
        T1Query.getForm().findField('P1').setValue('');
        var wh_no = T1Query.getForm().findField('P0').getValue();
        //物料分類
        Ext.Ajax.request({
            url: St_MatclassGet,
            method: reqVal_p,
            params: {
                p0: wh_no
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_matclass = data.etts;
                    if (tb_matclass.length > 0) {
                        for (var i = 0; i < tb_matclass.length; i++) {
                            st_matclass.add({ VALUE: tb_matclass[i].VALUE, COMBITEM: tb_matclass[i].COMBITEM });
                        }
                        if (tb_matclass.length == 1) {
                            //1筆資料時將
                            T1Query.getForm().findField('P1').setValue(tb_matclass[0].VALUE);
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function setDefaultPoTime() {
        // 訂單日期區間
        Ext.Ajax.request({
            url: '/api/BD0021/GetPoTime',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_poTime = data.etts;
                    if (tb_poTime.length > 0) {
                        T1Query.getForm().findField('D0').setValue(tb_poTime[0].VALUE);
                        T1Query.getForm().findField('D1').setValue(tb_poTime[0].TEXT);
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    setComboWhNo();
    setComboMatClass();
    setDefaultPoTime();

    //T1Model        //定義有多少欄位參數
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'PO_NO', direction: 'DESC' }, { property: 'WH_NO', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0021/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            load: function (store, options) {   //設定匯出是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('export').setDisabled(false);
                } else {
                    msglabel('查無符合條件的資料!');
                    Ext.getCmp('export').setDisabled(true);
                }
            }
        }
    });

    var T11Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0021/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }, listeners: {
            beforeload: function (store, options) {
                if (T1LastRec) {
                    var np = {
                        p0: T1LastRec.data.PO_NO
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
                else {
                    T11Store.removeAll();
                    return false;
                }
            }
        }
    });
    function T11Load() {
        T11Store.load({
            params: {
                start: 0
            }
        });
    }
    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        T1Store.getProxy().setExtraParam("d0", T1Query.getForm().findField('D0').rawValue);
        T1Store.getProxy().setExtraParam("d1", T1Query.getForm().findField('D1').rawValue);
        T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());

        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                id: 'export',
                text: '匯出',
                disabled: true,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '訂單紀錄查詢' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });
                    p.push({ name: 'd0', value: T1Query.getForm().findField('D0').rawValue });
                    p.push({ name: 'd1', value: T1Query.getForm().findField('D1').rawValue });

                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            },
            {
                text: '未進貨報表',
                id: 'btnReport',
                name: 'btnReport',
                handler: function () {
                    showReport();
                }
            }
        ]
    });

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
                text: "訂單號碼",
                dataIndex: 'PO_NO',
                style: 'text-align:left',
                align: 'left',
                width: 130
            },
            {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAME',
                style: 'text-align:left',
                align: 'left',
                width: 100
            },
            {
                text: "訂單日期",
                dataIndex: 'PO_TIME',
                style: 'text-align:left',
                align: 'left',
                width: 80
            },
            {
                text: "合約識別碼",
                dataIndex: 'M_CONTID',
                style: 'text-align:left',
                align: 'left',
                width: 80
            },
            {
                text: "狀態",
                dataIndex: 'PO_STATUS',
                style: 'text-align:left',
                align: 'left',
                width: 100
            },
            {
                text: "物料分類",
                dataIndex: 'MAT_CLASS',
                style: 'text-align:left',
                align: 'left',
                width: 80
            },
            {
                text: "庫房別",
                dataIndex: 'WH_NO',
                style: 'text-align:left',
                align: 'left',
                width: 70
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                T11Load();
            }
        }
    });

    // 查詢結果資料列表
    var T11Grid = Ext.create('Ext.grid.Panel', {
        store: T11Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
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
            },
            {
                text: "品名",
                dataIndex: 'MMNAME',
                style: 'text-align:left',
                align: 'left',
                width: 150
            },
            {
                text: "申購計量單位",
                dataIndex: 'M_PURUN',
                style: 'text-align:left',
                align: 'left',
                width: 150
            },
            {
                text: "訂單單價",
                dataIndex: 'PO_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 150
            },
            {
                text: "訂單數量",
                dataIndex: 'PO_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 150
            },
            {
                text: "總金額",
                dataIndex: 'PO_AMT',
                style: 'text-align:left',
                align: 'right',
                width: 150
            },
            {
                text: "院內最小計量單位",
                dataIndex: 'UPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 130
            },
            {
                text: "採購計量單位轉換率",
                dataIndex: 'UNIT_SWAP',
                style: 'text-align:left',
                align: 'right',
                width: 100
            },
            {
                text: "折讓比",
                dataIndex: 'M_DISCPERC',
                style: 'text-align:left',
                align: 'right',
                width: 100
            },
            {
                text: "二次折讓意願",
                dataIndex: 'ISWILLING',
                style: 'text-align:left',
                align: 'left',
                width: 130
            },
            {
                text: "二次折讓數量",
                dataIndex: 'DISCOUNT_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 130
            },
            {
                text: "二次折讓成本價",
                dataIndex: 'DISC_COST_UPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            },
            {
                text: "已交數量",
                dataIndex: 'DELI_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
        }
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + T1Query.getForm().findField('P0').getValue() + '&p1=' + T1Query.getForm().findField('P1').getValue() + '&p2=' + T1Query.getForm().findField('P2').getValue() + '&p3=' + T1Query.getForm().findField('P3').getValue() + '&p4=' + T1Query.getForm().findField('P4').getValue() + '&d0=' + T1Query.getForm().findField('D0').rawValue + '&d1=' + T1Query.getForm().findField('D1').rawValue + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
        items: [
            {
                region: 'north',
                itemId: 't1Grid',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                height: '45%',
                items: [T1Grid]
            }, {
                region: 'north',
                itemId: 't11Grid',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                height: '55%',
                items: [T11Grid]
            }
        ]
    });

    T1Query.getForm().findField('P0').focus();
    Ext.getCmp('export').setDisabled(true);
});