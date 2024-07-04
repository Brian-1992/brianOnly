Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {

    var reportUrl = '/Report/F/FA0082.aspx';
    // var T1Get = '/api/FA0082/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "各衛星庫房間互撥明細報表";
    var T1GetExcel = '/api/FA0082/Excel';

    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var fid = "1";
    //id = Ext.getUrlParam('fid');
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_whno = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: fid
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function () {
                var combo = Ext.getCmp('P4');
                combo.setValue(this.first().data.VALUE);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0082/GetWhnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0082/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_flowid = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "0201", "TEXT": "申請中" },
            { "VALUE": "0202", "TEXT": "調出中" },
            { "VALUE": "0203", "TEXT": "調入中" },
            { "VALUE": "0204", "TEXT": "取消調撥中" },
            { "VALUE": "0299", "TEXT": "已結案" }
        ]
    });

    var mLabelWidth = 90;
    var mWidth = 230;
    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P3',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: mLabelWidth,
        width: mWidth,
        padding: '0 4 0 4',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/FA0082/GetMMCodeDocd', //指定查詢的Controller路徑
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Query.getForm().findField('P2').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    // 查詢欄位
    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    function get6monthday() {
        var rtnDay = addMonths(new Date(), -6);
        return rtnDay
    }
    function get1monthday() {
        var rtnDay = addMonths(new Date(), -1);
        return rtnDay
    }
    function addMonths(date, months) {
        date.setMonth(date.getMonth() + months);
        return date;
    }
    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        layout: 'hbox',
        border: false,
        //autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
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
                id: 'PanelP0',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'datefield',
                    fieldLabel: '調撥申請區間',
                    labelAlign: 'right',
                    name: 'P0',
                    id: 'P0',
                    vtype: 'dateRange',
                    dateRange: { end: 'P1' },
                    labelWidth: 90,
                    width: 230,
                    value: get6monthday()
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelAlign: 'right',
                    labelWidth: 10,
                    name: 'P1',
                    id: 'P1',
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'P0' },
                    width: 150,
                    value: getToday()
                }, {
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    name: 'P2',
                    id: 'P2',
                    store: st_matclass,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                }, T1QueryMMCode, //P3
                {
                    xtype: 'combo',
                    fieldLabel: '狀態',
                    name: 'P6',
                    id: 'P6',
                    store: st_flowid,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    labelAlign: 'right',
                    labelWidth: 60,
                    width:200,
                    listeners: {
                        select: function (c, r, i, e) {
                            if (r.get('VALUE') == '0299') {
                                T1Grid.down('#print').setDisabled(false);
                            }
                        }
                    }
                }]
            }, {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'combo',
                    fieldLabel: '庫房',
                    name: 'P4',
                    id: 'P4',
                    store: st_whno,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth
                }, {
                    xtype: 'radiogroup',
                    name: 'whType',
                    //fieldLabel: '報表類別', copy from AA0079
                    //labelWidth: 80,
                    items: [
                        { boxLabel: '全部', width: 45, name: 'whType', inputValue: 'all', checked: true },
                        { boxLabel: '調入', width: 45, name: 'whType', inputValue: 'towh' },
                        { boxLabel: '調出', width: 45, name: 'whType', inputValue: 'frwh' },
                    ]
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        T1Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                        if (fid == '1') {
                            f.findField('P4').setValue(st_whno.getAt(0).get('COMBITEM'));
                        }
                    }
                }]
            }]
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'FRWH', type: 'string' },
            { name: 'FRWH_N', type: 'string' },
            { name: 'TOWH', type: 'string' },
            { name: 'TOWH_N', type: 'string' },
            { name: 'MAT_CLASS_SUB', type: 'string' },
            { name: 'MAT_CLASS_SUB_N', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'MMNAME_CE', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'APPTIME_T', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'ACKTIME', type: 'string' },
            { name: 'ACKTIME_T', type: 'string' },
            { name: 'ACKQTY', type: 'string' },
            { name: 'APVTIME', type: 'string' },
            { name: 'APVTIME_T', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'FLOWUD', type: 'string' },
            { name: 'FLOWUD_N', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 30, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0082/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('whType').getValue()['whType'],
                    p6: T1Query.getForm().findField('P6').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
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
        emptyMsg: '沒有任何資料    <span style=\'color:red\'> 注意:狀態必須選擇已結案才能列印</span>',
        displayMsg: "顯示{0} - {1}筆,共{2}筆    <span style=\'color:red\'> 注意:狀態必須選擇已結案才能列印</span>",
        buttons: [
            {
                itemId: 'export', text: '匯出',
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '各衛星庫房間互撥明細報表.xls' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });
                    p.push({ name: 'p5', value: T1Query.getForm().findField('whType').getValue()['whType'] });
                    p.push({ name: 'p6', value: T1Query.getForm().findField('P6').getValue() });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');

                }
            }, {
                itemId: 'print', text: '列印', disabled: true, handler: function () {
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
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 110,
                sortable: true
            }, {
                text: "調出庫",
                dataIndex: 'FRWH_N',
                width: 120
            }, {
                text: "調入庫",
                dataIndex: 'TOWH_N',
                width: 120
            }, {
                text: "物料分類",
                dataIndex: 'MAT_CLASS_SUB_N',
                width: 80
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            }, {
                text: "品名",
                dataIndex: 'MMNAME_CE',
                width: 300
            }, {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 80
            }, {
                text: "申請日期",
                dataIndex: 'APPTIME_T',
                width: 80
            }, {
                text: "申請數量",
                dataIndex: 'APPQTY',
                style: 'text-align:left',
                width: 80, align: 'right'
            }, {
                text: "調入日期",
                dataIndex: 'ACKTIME_T',
                width: 80
            }, {
                text: "調入數量",
                dataIndex: 'ACKQTY',
                style: 'text-align:left',
                width: 80, align: 'right'
            }, {
                text: "調出日期",
                dataIndex: 'APVTIME_T',
                width: 80
            }, {
                text: "調出數量",
                dataIndex: 'APVQTY',
                style: 'text-align:left',
                width: 80, align: 'right'
            }, {
                text: "狀態",
                dataIndex: 'FLOWID_N',
                width: 80
            }, {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {

                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];


            }
        }
    }
    );

    function showReport() {
        if (!win) {
            var p0 = T1Query.getForm().findField('P0').rawValue == null ? '' : T1Query.getForm().findField('P0').rawValue;
            var p1 = T1Query.getForm().findField('P1').rawValue == null ? '' : T1Query.getForm().findField('P1').rawValue;
            var p2 = T1Query.getForm().findField('P2').getValue() == null ? '' : T1Query.getForm().findField('P2').getValue();
            var p3 = T1Query.getForm().findField('P3').getValue() == null ? '' : T1Query.getForm().findField('P3').getValue();
            var p4 = T1Query.getForm().findField('P4').getValue() == null ? '' : T1Query.getForm().findField('P4').getValue();
            var p5 = T1Query.getForm().findField('whType').getValue()['whType'];
            var p6 = T1Query.getForm().findField('P6').getValue() == null ? '' : T1Query.getForm().findField('P6').getValue();

            var qstring = '?fid=' + fid + '&p0=' + p0 + '&p1=' + p1 + '&p2=' + p2 + '&p3=' + p3 + '&p4=' + p4 + '&p5=' + p5 + '&p6=' + p6;

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + qstring + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
    //view 
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
        }]
    });

    var winActWidth = viewport.width - 10;
    var winActHeight = viewport.height - 10;


    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();

    if (fid == '1') {
        T1Query.getForm().findField('P4').setDisabled(true);
        T1Query.getForm().findField('whType').setDisabled(true);
    }

});
