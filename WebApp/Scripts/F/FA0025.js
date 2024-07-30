﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name2 = "衛星庫房基準量查詢";
    var reportUrl = '/Report/A/AA0105.aspx';

    var T1RecLength = 0;
    var T1LastRec = null;

    var T1GetExcel = '../../../api/FA0025/Excel';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var WH_NOComboGet = '../../../api/FA0025/GetWH_NOComboOne';  //庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫)

    // 庫房代碼
    var wh_noStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
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
                    var combo_P0 = T1Query.getForm().findField('P0');
                    if (tb_wh_no.length == 1) {
                        //1筆資料時將
                        wh_noStore.add({ WH_NO: tb_wh_no[0].WH_NO, WH_NAME: tb_wh_no[0].WH_NAME });
                        combo_P0.setValue(tb_wh_no[0].WH_NO);


                    }
                    else {
                        combo_P0.setDisabled(false);
                        /////}
                        if (tb_wh_no.length > 0) {
                            for (var i = 0; i < tb_wh_no.length; i++) {
                                wh_noStore.add({ WH_NO: tb_wh_no[i].WH_NO, WH_NAME: tb_wh_no[i].WH_NAME });
                            }
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    //物料分類Store
    var st_MatClass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0025/GetMatClassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var MatClassCount = store.getCount();
                var combo_P3 = T1Query.getForm().findField('P3');
                if (MatClassCount == 1) {
                    //1筆資料時將                    
                    //combo_P3.setDisabled(true);
                    combo_P3.setValue(store.getAt(0).get('VALUE'));
                }
                else {
                    combo_P3.setDisabled(false);
                }
            }
        },
        autoLoad: true
    });

    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P1',
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        width: 300,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/FA0025/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Query.getForm().findField('P0').getValue()  //P0是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
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
            bodyStyle: 'padding: 3px 5px;',
            items: [
                {
                    xtype: 'combo',
                    store: wh_noStore,
                    fieldLabel: '庫房代碼',
                    name: 'P0',
                    id: 'P0',
                    queryMode: 'local',
                    fieldCls: 'required',
                    allowBlank: false,
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
                },
              
                 {
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    store: st_MatClass,
                    name: 'P3',
                    id: 'P3',
                    labelWidth: 80,
                    width: 210,
                    queryMode: 'local',
                    fieldCls: 'required',
                    allowBlank: false,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    //listConfig:
                    //{
                    //    width: 180
                    //},
                    padding: '0 4 0 4',
                    matchFieldWidth: false
                },
                T1QuryMMCode,     // 院內碼
                {
                    xtype: 'panel',
                    id: 'PanelP2',
                    border: false,
                    layout: 'hbox',
                    bodyStyle: 'padding: 3px 5px;',
                    items: [
                        {
                            xtype: 'radiogroup',
                            anchor: '40%',
                            labelWidth: 20,
                            width: 210,
                            name: 'P2',
                            id: 'P2',
                            items: [
                                {
                                    boxLabel: '查詢基準量',
                                    width: 80,
                                    name: 'P2',
                                    inputValue: 1,
                                    checked: true,
                                    handler: function () {
                                        T1Grid.getColumns()[17].setVisible(true);
                                        T1Grid.getColumns()[18].setVisible(true);
                                        T1Grid.getColumns()[19].setVisible(true);
                                        T1Grid.getColumns()[20].setVisible(true);
                                    }
                                },
                                {
                                    boxLabel: '查詢超出',
                                    width: 80,
                                    name: 'P2',
                                    inputValue: 2,
                                    handler: function () {
                                        T1Grid.getColumns()[17].setVisible(false);
                                        T1Grid.getColumns()[18].setVisible(false);
                                        T1Grid.getColumns()[19].setVisible(false);
                                        T1Grid.getColumns()[20].setVisible(false);
                                    }
                                }
                            ],
                            padding: '0 4 0 4'
                        },
                    ]
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    style: 'margin:0px 5px;',
                    handler: T1Load
                },
                {
                    xtype: 'button',
                    text: '列印',
                    style: 'margin:0px 5px;',
                    handler: function () {
                    if ((T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null) ||
                        (T1Query.getForm().findField('P3').getValue() == "" || T1Query.getForm().findField('P3').getValue() == null)) {
                           Ext.Msg.alert('訊息', '需填庫房代碼及物料分類才能查詢');
                    }
                    else {
                           showReport();
                    }
                }
            },
            {
                xtype: 'button',
                text: '清除',
                style: 'margin:0px 5px;',
                handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                msglabel('訊息區:');
            }
          }
          ]
        }]
    });

    Ext.define('AA0105_Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },        // 自訂 UNIT_CODE + UI_CHANAME
            { name: 'SAFE_DAY', type: 'string' },
            { name: 'OPER_DAY', type: 'string' },
            { name: 'SHIP_DAY', type: 'string' },
            { name: 'SAFE_QTY', type: 'string' },
            { name: 'OPER_QTY', type: 'string' },
            { name: 'SHIP_QTY', type: 'string' },
            { name: 'DAVG_USEQTY', type: 'string' },
            { name: 'HIGH_QTY', type: 'string' },
            { name: 'LOW_QTY', type: 'string' },
            { name: 'MIN_ORDQTY', type: 'string' },
            { name: 'SUPPLY_WHNO', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'AA0105_Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'WH_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0025/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T1Load() {
        if ((T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null) ||
            (T1Query.getForm().findField('P3').getValue() == "" || T1Query.getForm().findField('P3').getValue() == null)) {

            Ext.Msg.alert('訊息', '需填庫房代碼及物料分類才能查詢');
        } else {
            T1Tool.moveFirst();
        }
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
                itemId: 'export',
                text: '匯出',
                handler: function () {
                    if ((T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null) ||
                        (T1Query.getForm().findField('P3').getValue() == "" || T1Query.getForm().findField('P3').getValue() == null)) {

                        Ext.Msg.alert('訊息', '需填庫房代碼及物料分類才能匯出');
                    } else {
                        var today = getTodayDate();
                        var p = new Array();
                        p.push({ name: 'FN', value: today + '_衛星庫房基準量.xls' });
                        p.push({ name: 'P0', value: T1Query.getForm().findField('P0').getValue() });
                        p.push({ name: 'P1', value: T1Query.getForm().findField('P1').getValue() });
                        p.push({ name: 'P2', value: T1Query.getForm().findField('P2').getValue().P2 });
                        p.push({ name: 'P3', value: T1Query.getForm().findField('P3').getValue() });
                        PostForm(T1GetExcel, p);
                    }
                }
            }
        ]
    });

    var getTodayDate = function () {
        var y = (new Date().getFullYear() - 1911).toString();
        var m = (new Date().getMonth() + 1).toString();
        var d = (new Date().getDate()).toString();
        m = m.length > 1 ? m : "0" + m;
        d = d.length > 1 ? d : "0" + d;
        return y + m + d;
    }

    // 查詢結果資料列表
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
        },
        {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            style: 'text-align:left',
            align: 'left',
            width: 80
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            style: 'text-align:left',
            align: 'left',
            width: 80
        }, {
            text: "安全日",
            dataIndex: 'SAFE_DAY',
            style: 'text-align:left',
            align: 'right',
            width: 70
        }, {
            text: "作業日",
            dataIndex: 'OPER_DAY',
            style: 'text-align:left',
            align: 'right',
            width: 70
        }, {
            text: "運補日",
            dataIndex: 'SHIP_DAY',
            style: 'text-align:left',
            align: 'right',
            width: 70
        }, {
            text: "基準量",
            dataIndex: 'HIGH_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "最低庫存量",
            dataIndex: 'LOW_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "最小撥補量",
            dataIndex: 'MIN_ORDQTY',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "安全量",
            dataIndex: 'SAFE_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "作業量",
            dataIndex: 'OPER_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "運補量",
            dataIndex: 'SHIP_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "日平均消耗量",
            dataIndex: 'DAVG_USEQTY',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            style: 'text-align:left',
            width: 100
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            style: 'text-align:left',
            width: 200
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            style: 'text-align:left',
            width: 200
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            style: 'text-align:left',
            width: 100
        }, {
            text: "累計撥發量",
            dataIndex: 'APL_INQTY',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "申請單號",
            dataIndex: 'DOCNO',
            style: 'text-align:left',
            align: 'right',
            width: 150
        }, {
            text: "申請量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "庫存單價",
            dataIndex: 'AVG_PRICE',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "超量原因",
            dataIndex: 'GTAPL_REASON',
            style: 'text-align:left',
            width: 200
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
            }
        }
    });

    function showReport() {
        if (!win) {
            var np = {
                p0: T1Query.getForm().findField('P0').getValue(),
                p1: T1Query.getForm().findField('P1').getValue(),
                p2: T1Query.getForm().findField('P2').getValue().P2,    //getValue()後是RadioGroup物件，需要再選擇該物件的屬性名稱
                p3: T1Query.getForm().findField('P3').getValue(),
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
        },
        ]
    });

    T1Grid.getColumns()[17].setVisible(false);
    T1Grid.getColumns()[18].setVisible(false);
    T1Grid.getColumns()[19].setVisible(false);
    T1Grid.getColumns()[20].setVisible(false);

    //T1Load(); // 進入畫面時自動載入一次資料
    //T1Query.getForm().findField('P0').focus();
});