Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Get = '/api/AB0126/All';
    var T2Get = '/api/AB0126/GetLotNo';
    var QtyGet = '/api/AB0126/GetInvQty';
    var QtySet = '/api/AB0126/SetQty';
    var QtySetBatch = '/api/AB0126/SetQtyBatch';
    var T3GetExcel = '../../../api/AB0126/Excel';
    var T1Name = "藥局調撥作業";

    var T1Rec = 0;
    var T1LastRec = null;

    var G_TOWH = '';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var towh_store = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                //var np = {
                //    p0: popform.getForm().findField('MMCODE').getValue()
                //};
                //Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AB0126/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }// , autoLoad: true
    });

    var frwh_store = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: G_TOWH
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0126/GetFrwhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }//, autoLoad: true
    });

    var defaultColumns = [{
        xtype: 'rownumberer',
        width: 30,
        align: 'Center',
        labelAlign: 'Center'
    }, {
        text: "院內碼",
        dataIndex: 'MMCODE',
        width: 100
    }, {
        text: "中文品名",
        dataIndex: 'MMNAME_C',
        width: 150
    }, {
        text: "英文品名",
        dataIndex: 'MMNAME_E',
        width: 150
    }
    ];

    var query_isrestrict = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        data: [
            { "VALUE": "", "TEXT": "", "COMBITEM": "" },
            { "VALUE": "Y", "TEXT": "是", "COMBITEM": "Y 是" },
            { "VALUE": "N", "TEXT": "否", "COMBITEM": "N 否" }
        ]
    });

    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P0',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 100, //限制一次最多顯示10筆
        queryUrl: '/api/AB0126/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        width: 170,
        matchFieldWidth: false,
        listConfig: {
            width: 250
        },
        getDefaultParams: function () { //查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 140;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
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
                T1QueryMMCode,
                //{
                //    xtype: 'textfield',
                //    fieldLabel: '院內碼',
                //    name: 'P0',
                //    id: 'P0',
                //    enforceMaxLength: true,
                //    maxLength: 13,
                //    padding: '0 4 0 4'
                //},
                {
                    xtype: 'textfield',
                    fieldLabel: '中文品名',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 250,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '英文品名',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    maxLength: 250,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'combo',
                    fieldLabel: '管制藥',
                    name: 'P3',
                    id: 'P3',
                    store: query_isrestrict,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    editable: false,
                    allowBlank: true
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
                        f.findField('p0').focus();
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'WEXP_ID', type: 'string' },
            { name: 'EXTRA_DATA', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: T1Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {
                // store載入完成後,將EXTRA_DATA解成額外欄位資料後再載入store內
                var reArray = [];
                var colName = [];
                for (var i = 4; i < T1Grid.columns.length; i++) {
                    colName.push(T1Grid.columns[i].dataIndex)
                }

                for (var i = 0; i < store.data.length; i++) {
                    // 每列的基本欄位
                    var itemObj = {
                        "MMCODE": store.data.items[i].data['MMCODE'],
                        "MMNAME_C": store.data.items[i].data['MMNAME_C'],
                        "MMNAME_E": store.data.items[i].data['MMNAME_E'],
                        "WEXP_ID": store.data.items[i].data['WEXP_ID']
                    };
                    // 組合後面額外的欄位
                    var extraData = store.data.items[i].data['EXTRA_DATA'].split(',');
                    for (var j = 0; j < extraData.length; j++) {
                        if (extraData[j].split('.')[0])
                            itemObj[colName[j]] = extraData[j];
                        else
                            itemObj[colName[j]] = '0' + extraData[j];
                    }

                    reArray.push(itemObj);
                }

                store.loadData(reArray);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'LOT_NO', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'APP_QTY', type: 'string' }
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 100, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'EXP_DATE', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: T2Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件代入參數
                var np = {
                    p0: popform.getForm().findField('FRWH').getValue(),
                    p1: popform.getForm().findField('MMCODE').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {

            }
        }
    });

    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
            { name: 'APP_QTY', type: 'string' },
            { name: 'LOT_NO', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'CHECK_RESULT', type: 'string' }
        ]
    });
    var T3Store = Ext.create('Ext.data.Store', {
        model: 'T3Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
            }
        }
    })

    //#region popWinForm
    var callableWin = null;
    popWinForm = function (url) {
        var strUrl = url;
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                        if (url == 'GA0004') {
                            T31Load();
                        }
                    }
                }]
            });
            var title = url == 'AB0133?isARMY=Y' ? '調撥資料查詢' : '';
            callableWin = GetPopWin(viewport, popform, title, viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
    }
    //#endregion

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                xtype: 'button',
                text: '調撥資料查詢',
                handler: function () {
                    popWinForm('AB0133?isARMY=Y');
                }
            }, {
                itemId: 'newRec',
                text: '調撥',
                disabled: false,
                handler: function () {
                    popEditForm();
                }
            }, , {
                itemId: 'newRecBatch',
                text: '批次調撥',
                disabled: false,
                handler: function () {
                    popImportForm();
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
        enableColumnMove: false,
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
                width: 100
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 150
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 150
            }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });

    var callableWin = null;
    var popform = null;
    popEditForm = function () {
        if (!callableWin) {
            var T2FormMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
                name: 'MMCODE',
                fieldLabel: '院內碼',
                fieldCls: 'required',
                allowBlank: false,
                labelAlign: 'right',
                limit: 10, //限制一次最多顯示10筆
                queryUrl: '/api/AB0126/GetMMCode', //指定查詢的Controller路徑
                extraFields: ['MMNAME_C', 'MMNAME_E', 'WEXP_ID'], //查詢完會回傳的欄位
                //getDefaultParams: function () { //查詢時Controller固定會收到的參數
                //    return {
                //        p1: ''
                //    };
                //},
                listeners: {
                    select: function (c, r, i, e) {
                        setT2Form(r);
                    }
                }
            });

            popform = Ext.create('Ext.form.Panel', {
                id: 'itemDetail',
                height: '100%',
                // layout: 'fit',
                closable: false,
                border: true,
                fieldDefaults: {
                    labelAlign: 'right',
                    labelWidth: 100,
                    width: '100%'
                },
                items: [
                    {
                        xtype: 'container',
                        layout: 'vbox',
                        padding: '2vmin',
                        scrollable: true,
                        items: [
                            T2FormMMCode, {
                                xtype: 'displayfield',
                                fieldLabel: '中文品名',
                                name: 'MMNAME_C',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '英文品名',
                                name: 'MMNAME_E',
                                readOnly: true
                            }, {
                                xtype: 'container',
                                layout: 'hbox',
                                width: '100%',
                                items: [
                                    {
                                        xtype: 'combo',
                                        fieldLabel: '調入庫房',
                                        name: 'TOWH',
                                        fieldCls: 'required',
                                        allowBlank: false,
                                        queryMode: 'local',
                                        store: towh_store,
                                        displayField: 'TEXT',
                                        valueField: 'VALUE',
                                        matchFieldWidth: false,
                                        width: '50%',
                                        forceSelection: true,
                                        listConfig: {
                                            width: 230
                                        },
                                        listeners: {
                                            select: function (combo, record, index) {
                                                popform.getForm().findField('FRWH').setValue('');
                                                G_TOWH = popform.getForm().findField('TOWH').getValue();
                                                frwh_store.load();
                                                if (popform.getForm().findField('MMCODE').getValue()) {
                                                    ShowWhInvQty(popform.getForm().findField('MMCODE').getValue(),
                                                        popform.getForm().findField('FRWH').getValue(),
                                                        popform.getForm().findField('TOWH').getValue(),
                                                        popform.getForm().findField('APP_QTY').getValue()
                                                    );
                                                }
                                            }
                                        }
                                    }, {
                                        xtype: 'displayfield',
                                        fieldLabel: '庫存量',
                                        name: 'TOWH_QTY',
                                        labelWidth: 60,
                                        labelSeparator: '',
                                        readOnly: true
                                    }
                                ]
                            }, {
                                xtype: 'container',
                                layout: 'hbox',
                                width: '100%',
                                items: [
                                    {
                                        xtype: 'combo',
                                        fieldLabel: '調出庫房',
                                        name: 'FRWH',
                                        fieldCls: 'required',
                                        allowBlank: false,
                                        queryMode: 'local',
                                        store: frwh_store,
                                        displayField: 'TEXT',
                                        valueField: 'VALUE',
                                        matchFieldWidth: false,
                                        width: '50%',
                                        forceSelection: true,
                                        listConfig: {
                                            width: 230
                                        },
                                        listeners: {
                                            select: function (combo, record, index) {
                                                if (popform.getForm().findField('MMCODE').getValue()) {
                                                    ShowWhInvQty(popform.getForm().findField('MMCODE').getValue(),
                                                        popform.getForm().findField('FRWH').getValue(),
                                                        popform.getForm().findField('TOWH').getValue(),
                                                        popform.getForm().findField('APP_QTY').getValue()
                                                    );
                                                    // if (popform.getForm().findField('WEXP_ID').getValue() == 'Y')
                                                    T2Store.load();
                                                }
                                            }
                                        }
                                    }, {
                                        xtype: 'displayfield',
                                        fieldLabel: '庫存量',
                                        name: 'FRWH_QTY',
                                        labelWidth: 60,
                                        labelSeparator: '',
                                        readOnly: true
                                    }
                                ]
                            }, {
                                xtype: 'numberfield',
                                fieldLabel: '調撥量',
                                name: 'APP_QTY',
                                id: 'APP_QTY',
                                minValue: 0,
                                allowDecimals: false, //是否允許輸入小數
                                keyNavEnabled: false,
                                mouseWheelEnabled: false,
                                fieldCls: 'required',
                                allowBlank: false,
                                readOnly: false,
                                width: '50%',
                                listeners: {
                                    blur: function (_this, event, eOpts) {
                                        // 如果超過最大值，自動設定為最大值(排除最大值為負數)
                                        if (_this.getValue() && _this.maxValue && (_this.getValue() > _this.maxValue) && (_this.maxValue >= 0)) {
                                            _this.setValue(_this.maxValue);
                                        }

                                        if (popform.getForm().findField('MMCODE').getValue()) {
                                            ShowWhInvQty(popform.getForm().findField('MMCODE').getValue(),
                                                popform.getForm().findField('FRWH').getValue(),
                                                popform.getForm().findField('TOWH').getValue(),
                                                popform.getForm().findField('APP_QTY').getValue()
                                            );

                                            UpdateLotAppqty(popform.getForm().findField('APP_QTY').getValue());
                                        }
                                    },
                                    change: function (_this, event, eOpts) {
                                        // 如果超過最大值，自動設定為最大值(排除最大值為負數)
                                        if (_this.getValue() && _this.maxValue && (_this.getValue() > _this.maxValue) && (_this.maxValue >= 0)) {
                                            _this.setValue(_this.maxValue);
                                        }

                                        if (popform.getForm().findField('MMCODE').getValue()) {
                                            ShowWhInvQty(popform.getForm().findField('MMCODE').getValue(),
                                                popform.getForm().findField('FRWH').getValue(),
                                                popform.getForm().findField('TOWH').getValue(),
                                                popform.getForm().findField('APP_QTY').getValue()
                                            );

                                            UpdateLotAppqty(popform.getForm().findField('APP_QTY').getValue());
                                        }
                                    }
                                }
                            }, {
                                xtype: 'hidden',
                                name: 'WEXP_ID'
                            }
                        ]
                    }, {
                        xtype: 'grid',
                        id: 'itemdetailgrid',
                        store: T2Store,
                        height: 180,
                        hidden: true,
                        plain: true,
                        loadingText: '處理中...',
                        loadMask: true,
                        cls: 'T1',
                        columns: [
                            {
                                xtype: 'rownumberer'
                            },
                            {
                                text: "批號",
                                dataIndex: 'LOT_NO',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                sortable: false,
                                editor: {
                                    xtype: 'textfield',
                                    name: 'LOT_NO',
                                    maxLength: 20,
                                    allowBlank: false
                                }
                            }, {
                                xtype: 'datecolumn',
                                text: "效期",
                                dataIndex: 'EXP_DATE',
                                format: 'Xmd',
                                align: 'left',
                                style: 'text-align:center',
                                width: 80,
                                sortable: false,
                                editor: {
                                    xtype: 'datefield',
                                    name: 'EXP_DATE',
                                    maskRe: /[0-9]/,
                                    format: 'Xmd',
                                    regex: /^([0-9]{7})$/,
                                    allowBlank: false,
                                    regexText: '格式需為yyymmdd,例如1080101',
                                    renderer: function (value, meta, record) {
                                        return Ext.util.Format.date(value, 'Xmd');
                                    },
                                    listeners: {
                                        focus: function (field, event, eOpts) {
                                            if (!field.isExpanded) {
                                                setTimeout(function () {
                                                    field.expand();
                                                }, 300);
                                            }
                                        }
                                    }
                                }
                            }, {
                                text: "庫存量",
                                dataIndex: 'INV_QTY',
                                align: 'right',
                                style: 'text-align:center',
                                width: 100,
                                sortable: false
                            }, {
                                text: "調撥量",
                                dataIndex: 'APP_QTY',
                                align: 'right',
                                style: 'text-align:center',
                                width: 100,
                                sortable: false,
                                editor: {
                                    xtype: 'numberfield',
                                    name: 'APP_QTY',
                                    value: 1,
                                    minValue: 0,
                                    allowDecimals: false, //是否允許輸入小數
                                    keyNavEnabled: false,
                                    mouseWheelEnabled: false,
                                    allowBlank: true,
                                    validator: function (val) {
                                        if (val < 0)
                                            return '數量必須大於等於0';
                                        return true;
                                    }
                                }
                            }
                        ],
                        plugins: [
                            Ext.create('Ext.grid.plugin.CellEditing', {
                                clicksToEdit: 1,//控制點擊幾下啟動編輯
                                listeners: {
                                    beforeedit: function (editor, context, eOpts) {
                                        if (context.field != 'APP_QTY') {
                                            // 有庫存量則視為來自MI_WEXPINV的資料,不可修改批號和效期
                                            if (context.record.data.INV_QTY)
                                                return false;
                                            else
                                                return true;
                                        }
                                        else
                                            return true;
                                    },
                                    validateedit: function (editor, context, eOpts) {
                                    }
                                }
                            })
                        ],
                        listeners: {
                            selectionchange: function (model, records) {
                                if (records.length > 0) {
                                    // 只能刪除新增的資料
                                    if (records[0].data.INV_QTY)
                                        popform.down('#btn_delExp').setDisabled(true);
                                    else
                                        popform.down('#btn_delExp').setDisabled(false);
                                }
                            }
                        }
                    }, {
                        xtype: 'container',
                        layout: 'hbox',
                        width: '100%',
                        items: [
                            {
                                xtype: 'button',
                                text: '新增',
                                id: 'btn_newExp',
                                name: 'btn_newExp',
                                hidden: true,
                                handler: function () {
                                    // 原則上Grid只能有一筆有APP_QTY的資料,這裡只允許新增一筆額外資料
                                    //if (T2Store.findExact('INV_QTY', '') < 0) {
                                        var newRec = T2Model.create({
                                            LOT_NO: '',
                                            EXP_DATE: '',
                                            INV_QTY: '',
                                            APP_QTY: ''
                                        });
                                        T2Store.insert(T2Store.count(), newRec);
                                    //}
                                }
                            }, {
                                xtype: 'button',
                                text: '刪除',
                                id: 'btn_delExp',
                                name: 'btn_delExp',
                                hidden: true,
                                handler: function () {
                                    Ext.MessageBox.confirm('刪除', '是否確定刪除?', function (btn, text) {
                                        if (btn === 'yes') {
                                            var pos = popform.down('#itemdetailgrid').getSelectionModel().getCurrentPosition();
                                            if (typeof (pos) != 'undefined') {
                                                T2Store.removeAt(pos.rowIdx);
                                                popform.down('#btn_delExp').setDisabled(true);
                                            }
                                        }
                                    });
                                }
                            }

                        ]
                    }
                ],
                buttons: [{
                    text: '<font size="3vmin">調撥確認</font>',
                    height: '6vmin',
                    handler: function () {
                        if (popform.getForm().isValid()) {
                            var store = T2Store.data.items;

                            if (popform.getForm().findField('APP_QTY').getValue() <= 0) {
                                Ext.Msg.alert('錯誤', '調撥數量需大於零，請重新確認');
                                return;
                            }

                            var temp = popform.getForm().findField('FRWH_QTY').getValue();
                            var af_frqty = Number(temp.split('=')[1]);
                            if (af_frqty < 0 || isNaN(af_frqty)) {
                                Ext.Msg.alert('錯誤', '調出庫房庫存不可為負，請重新確認');
                                return;
                            }

                            //if (popform.getForm().findField('WEXP_ID').getValue() == 'Y') {
                            for (var i = 0; i < store.length; i++) {
                                if (store[i].data.INV_QTY == '' && store[i].data.APP_QTY > 0) {
                                    // 新增的項目效期欄位是否都有填寫
                                    if (store[i].data.EXP_DATE == '') {
                                        Ext.Msg.alert('錯誤', '效期未填寫完成，請重新確認');
                                        return;
                                    }
                                }
                            }
                            //}

                            Ext.MessageBox.confirm('調撥', '是否確定調撥' + popform.getForm().findField('MMCODE').getValue()
                                + '調撥量' + popform.getForm().findField('APP_QTY').getValue() + '?', function (btn, text) {
                                    if (btn === 'yes') {
                                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });

                                        var list = [];
                                        var totAPP_QTY = 0;
                                        var validCnt = 0; // Grid中只能填寫一筆調撥量
                                        for (var i = 0; i < store.length; i++) {

                                            if (store[i].data.APP_QTY > 0) { // 排除調撥量未填的資料
                                                //if (store[i].data.INV_QTY != '') {
                                                //    if (store[i].data.APP_QTY > store[i].data.INV_QTY) {
                                                //        Ext.Msg.alert('訊息', '批號' + store[i].data.LOT_NO + '效期' + Ext.util.Format.date(store[i].data.EXP_DATE, 'Xmd') + ',調撥量不可大於庫存量');
                                                //        return;
                                                //    }
                                                //}
                                                var item = {
                                                    LOT_NO: store[i].data.LOT_NO,
                                                    EXP_DATE: Ext.util.Format.date(store[i].data.EXP_DATE, 'Xmd'),
                                                    APP_QTY: store[i].data.APP_QTY
                                                }
                                                list.push(item);
                                                validCnt++;
                                                totAPP_QTY += parseInt(store[i].data.APP_QTY);
                                            }
                                        }

                                        //if (list.length == 0) {
                                        //    Ext.Msg.alert('訊息', '尚未填寫任何批號效期調撥量，請確認!');
                                        //    return;
                                        //}
                                        //else if (validCnt > 1)
                                        //{
                                        //    Ext.Msg.alert('訊息', '一筆調撥資料只能填寫一筆批號效期調撥量!');
                                        //    return;
                                        //}
                                        if (totAPP_QTY > 0) {
                                            if (totAPP_QTY != popform.getForm().findField('APP_QTY').getValue()) {
                                                Ext.Msg.alert('訊息', '批號效期調撥量需等於本次總調撥量!');
                                                return;
                                            }
                                        }

                                        myMask.show();
                                        Ext.Ajax.request({
                                            url: QtySet,
                                            method: reqVal_p,
                                            params: {
                                                mmcode: popform.getForm().findField('MMCODE').getValue(),
                                                frwh: popform.getForm().findField('FRWH').getValue(),
                                                towh: popform.getForm().findField('TOWH').getValue(),
                                                app_qty: popform.getForm().findField('APP_QTY').getValue(),
                                                wexp_id: popform.getForm().findField('WEXP_ID').getValue(),
                                                list: Ext.util.JSON.encode(list)
                                            },
                                            success: function (response) {
                                                myMask.hide();
                                                var data = Ext.decode(response.responseText);
                                                if (data.success) {
                                                    Ext.MessageBox.alert('訊息', '調撥完成,已建立申請單' + data.msg + ',可至[調撥資料查詢]檢視結果.');
                                                    callableWin.destroy();
                                                    callableWin = null;
                                                    T1Load();
                                                } else {
                                                    Ext.MessageBox.alert('錯誤', data.msg);
                                                }
                                            },
                                            failure: function (response, options) {
                                                myMask.hide();
                                            }
                                        });
                                    }
                                });
                        } else {
                            Ext.Msg.alert('提醒', '欄位格式不正確,請重新確認');
                        }
                    }
                }, {
                    id: 'winclosed',
                    disabled: false,
                    text: '<font size="3vmin">關閉</font>',
                    height: '6vmin',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                    }
                }]
            });

            callableWin = GetPopWin(viewport, popform, '調撥', 500, 620);
        }
        callableWin.show();

        if (T1LastRec) {
            setT2Form(T1LastRec);
        }
        else
            popform.reset();
    }

    var callableWinImport = null;
    var popformImport = null;
    popImportForm = function () {
        if (!callableWinImport) {
            popformImport = Ext.create('Ext.form.Panel', {
                id: 'itemDetailImport',
                height: '100%',
                // layout: 'fit',
                closable: false,
                border: true,
                fieldDefaults: {
                    labelAlign: 'right',
                    labelWidth: 100,
                    width: '100%'
                },
                items: [
                    {
                        xtype: 'container',
                        layout: 'vbox',
                        padding: '2vmin',
                        scrollable: true,
                        items: [
                            {
                                xtype: 'container',
                                layout: 'hbox',
                                width: '100%',
                                items: [
                                    {
                                        xtype: 'combo',
                                        fieldLabel: '調入庫房',
                                        name: 'TOWH',
                                        fieldCls: 'required',
                                        allowBlank: false,
                                        queryMode: 'local',
                                        store: towh_store,
                                        displayField: 'TEXT',
                                        valueField: 'VALUE',
                                        matchFieldWidth: false,
                                        width: '50%',
                                        forceSelection: true,
                                        listConfig: {
                                            width: 230
                                        },
                                        listeners: {
                                            select: function (combo, record, index) {
                                                popformImport.getForm().findField('FRWH').setValue('');
                                                Ext.getCmp('T3send').setDisabled(true);
                                                G_TOWH = popformImport.getForm().findField('TOWH').getValue();
                                                frwh_store.load();
                                                T3Store.removeAll();
                                            }
                                        }
                                    }
                                ]
                            }, {
                                xtype: 'container',
                                layout: 'hbox',
                                width: '100%',
                                items: [
                                    {
                                        xtype: 'combo',
                                        fieldLabel: '調出庫房',
                                        name: 'FRWH',
                                        fieldCls: 'required',
                                        allowBlank: false,
                                        queryMode: 'local',
                                        store: frwh_store,
                                        displayField: 'TEXT',
                                        valueField: 'VALUE',
                                        matchFieldWidth: false,
                                        width: '50%',
                                        forceSelection: true,
                                        listConfig: {
                                            width: 230
                                        },
                                        listeners: {
                                            select: function (combo, record, index) {
                                                Ext.getCmp('T3send').setDisabled(false);
                                                T3Store.removeAll();
                                            }
                                        }
                                    }
                                ]
                            }
                        ]
                    }, {
                        xtype: 'grid',
                        id: 'itemdetailgridImport',
                        store: T3Store,
                        height: 270,
                        plain: true,
                        loadingText: '處理中...',
                        loadMask: true,
                        cls: 'T1',
                        columns: [
                            {
                                xtype: 'rownumberer'
                            }, {
                                text: "院內碼",
                                dataIndex: 'MMCODE',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                sortable: false
                            }, {
                                text: "調撥量",
                                dataIndex: 'APPQTY',
                                align: 'right',
                                style: 'text-align:center',
                                width: 90,
                                sortable: false
                            }, {
                                text: "批號",
                                dataIndex: 'LOT_NO',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                sortable: false
                            }, {
                                // xtype: 'datecolumn',
                                text: "效期",
                                dataIndex: 'EXPDATE',
                                // format: 'Xmd',
                                align: 'left',
                                style: 'text-align:center',
                                width: 80,
                                sortable: false
                            }, {
                                text: "調入庫存量",
                                dataIndex: 'A_INV_QTY',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                sortable: false
                            }, {
                                text: "調出庫存量",
                                dataIndex: 'B_INV_QTY',
                                align: 'left',
                                style: 'text-align:center',
                                width: 100,
                                sortable: false
                            }, {
                                text: "",
                                dataIndex: 'CHECK_RESULT',
                                align: 'left',
                                style: 'text-align:center',
                                width: 300,
                                sortable: false
                            }
                        ],
                        listeners: {
                            selectionchange: function (model, records) {
                            }
                        }
                    }, {
                        xtype: 'container',
                        layout: 'hbox',
                        width: '100%',
                        items: [
                            {
                                xtype: 'button',
                                id: 'importTemp',
                                name: 'importTemp',
                                text: '範本', handler: function () {
                                    var p = new Array();
                                    PostForm(T3GetExcel, p);
                                    msglabel('訊息區:匯出完成');
                                }
                            }, {
                                xtype: 'filefield',
                                name: 'T3send',
                                id: 'T3send',
                                disabled: true,
                                buttonOnly: true,
                                buttonText: '匯入',
                                width: 40,
                                listeners: {
                                    change: function (widget, value, eOpts) {
                                        Ext.getCmp('T3insert').setDisabled(true);
                                        T3Store.removeAll();
                                        var files = event.target.files;
                                        if (!files || files.length == 0) return;
                                        var f = files[0];
                                        var ext = this.value.split('.').pop();
                                        if (!/^(xls|xlsx)$/.test(ext)) {
                                            Ext.MessageBox.alert('提示', '請選擇xlsx或xls檔案！');
                                            Ext.getCmp('T3send').fileInputEl.dom.value = '';
                                            msglabel("請選擇xlsx或xls檔案！");
                                        }
                                        else {
                                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                            myMask.show();
                                            var formData = new FormData();
                                            formData.append("file", f);
                                            formData.append("matclass", '01');
                                            formData.append("towh", popformImport.getForm().findField('TOWH').getValue());
                                            formData.append("frwh", popformImport.getForm().findField('FRWH').getValue());
                                            var ajaxRequest = $.ajax({
                                                type: "POST",
                                                url: "/api/AB0126/SendExcel",
                                                data: formData,
                                                processData: false,
                                                //必須false才會自動加上正確的Content-Type
                                                contentType: false,
                                                success: function (data, textStatus, jqXHR) {
                                                    if (!data.success) {
                                                        T3Store.removeAll();
                                                        Ext.MessageBox.alert("提示", data.msg);
                                                        msglabel("訊息區:");
                                                        Ext.getCmp('T3insert').setDisabled(true);
                                                        IsSend = false;
                                                    }
                                                    else {
                                                        msglabel("訊息區:檔案讀取成功");
                                                        T3Store.loadData(data.etts, false);
                                                        IsSend = true;
                                                        if (data.msg == "True") {
                                                            Ext.getCmp('T3insert').setDisabled(false);
                                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: blue; font-weight: bold\">成功</span>，可進行調撥確認動作。");
                                                        };
                                                        if (data.msg == "False") {
                                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: red; font-weight: bold\">失敗</span>，請依錯誤說明修改Excel檔。");
                                                        };
                                                    }
                                                    Ext.getCmp('T3send').fileInputEl.dom.value = '';
                                                    myMask.hide();
                                                },
                                                error: function (jqXHR, textStatus, errorThrown) {
                                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                                    Ext.getCmp('T3send').fileInputEl.dom.value = '';
                                                    Ext.getCmp('T3insert').setDisabled(true);
                                                    myMask.hide();

                                                }
                                            });
                                        }
                                    }
                                }
                            }

                        ]
                    }
                ],
                buttons: [{
                    text: '<font size="3vmin">批次調撥確認</font>',
                    id: 'T3insert',
                    disabled: true,
                    height: '6vmin',
                    handler: function () {
                        if (popformImport.getForm().isValid()) {
                            var store = T3Store.data.items;

                            Ext.MessageBox.confirm('批次調撥', '是否確定批次調撥?', function (btn, text) {
                                if (btn === 'yes') {
                                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });

                                    var list = [];
                                    for (var i = 0; i < store.length; i++) {

                                        if (store[i].data.APPQTY > 0) { // 排除調撥量未填的資料
                                            var item = {
                                                MMCODE: store[i].data.MMCODE,
                                                APPQTY: store[i].data.APPQTY,
                                                LOT_NO: store[i].data.LOT_NO,
                                                EXPDATE: store[i].data.EXPDATE
                                            }
                                            list.push(item);
                                        }
                                    }

                                    myMask.show();
                                    Ext.Ajax.request({
                                        url: QtySetBatch,
                                        method: reqVal_p,
                                        params: {
                                            towh: popformImport.getForm().findField('TOWH').getValue(),
                                            frwh: popformImport.getForm().findField('FRWH').getValue(),
                                            list: Ext.util.JSON.encode(list)
                                        },
                                        success: function (response) {
                                            myMask.hide();
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                Ext.MessageBox.alert('訊息', '調撥完成,已建立申請單' + data.msg + ',可至[調撥資料查詢]檢視結果.');
                                                callableWinImport.destroy();
                                                callableWinImport = null;
                                                T1Load();
                                            } else {
                                                Ext.MessageBox.alert('錯誤', data.msg);
                                            }
                                        },
                                        failure: function (response, options) {
                                            myMask.hide();
                                        }
                                    });
                                }
                            });
                        } else {
                            Ext.Msg.alert('提醒', '欄位格式不正確,請重新確認');
                        }
                    }
                }, {
                    id: 'winclosed',
                    disabled: false,
                    text: '<font size="3vmin">關閉</font>',
                    height: '6vmin',
                    handler: function () {
                        this.up('window').destroy();
                        callableWinImport = null;
                    }
                }]
            });

            callableWinImport = GetPopWin(viewport, popformImport, '批次調撥', 750, 600);
        }
        callableWinImport.show();

        towh_store.load();
        T3Store.removeAll();
    }

    function setFormT1a() {
        // T1Grid.down('#newRec').setDisabled(T1Rec === 0);
    }

    function getDefaultColumns() {
        T1Grid.reconfigure(null, defaultColumns);

        Ext.Ajax.request({
            url: '/api/AB0126/GetColumns',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {

                    if (data.etts.length > 0) {
                        var columns = data.etts;
                        setT1GridColumns(columns);

                        // 只有一家藥局時顯示訊息並只允許查詢
                        if (data.etts.length <= 1) {
                            T1Grid.down('#newRec').setDisabled(true);
                            Ext.Msg.alert('訊息', '此功能僅在有兩間藥局以上的情況才可使用調撥作業.');
                        }
                    }

                    T1Store.removeAll();
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function setT1GridColumns(columns) {
        for (var i = 0; i < columns.length; i++) {
            var column = Ext.create('Ext.grid.column.Column', {
                text: columns[i].TEXT + '(' + columns[i].DATAINDEX + ')',
                dataIndex: columns[i].DATAINDEX,
                width: 150,
                align: 'right',
                style: 'text-align:left',
                sortable: false,
                align: 'right'
            });

            T1Grid.headerCt.insert(T1Grid.columns.length, column);
            T1Grid.columns.push(column);
        }
        T1Grid.getView().refresh();
    }

    // 依照選擇的院內碼、出入庫房、數量顯示庫存量
    function ShowWhInvQty(pMmcode, pFrwh, pTowh, pAppqty) {
        Ext.Ajax.request({
            url: QtyGet,
            method: reqVal_p,
            params: {
                'MMCODE': pMmcode,
                'FRWH': pFrwh,
                'TOWH': pTowh,
                'APP_QTY': pAppqty
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var rec = data.etts[0];
                    var rtnResult = data.msg.split('^');
                    var strFrwhRtn = "";
                    var strTowhRtn = "";
                    // 顯示 進出貨檢視結果
                    if (rec.frwh_qty && pAppqty) {
                        strFrwhRtn = rec.frwh_qty + " - " + pAppqty + " = " + (rec.frwh_qty - pAppqty);
                    }

                    if (rec.towh_qty && pAppqty) {
                        strTowhRtn = rec.towh_qty + " + " + pAppqty + " = " + (rec.towh_qty + pAppqty);
                    }

                    popform.getForm().findField('FRWH_QTY').setValue(strFrwhRtn);
                    popform.getForm().findField('TOWH_QTY').setValue(strTowhRtn);

                    // 動態設定上限值
                    if (rec.frwh_qty && rec.frwh_qty >= 0) {
                        Ext.getCmp('APP_QTY').setMaxValue(rec.frwh_qty);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 依照填入的調撥量更新Grid內各批號的調撥量
    function UpdateLotAppqty(pAppqty) {
        var remainQty = pAppqty;
        for (var i = 0; i < T2Store.data.length; i++) {
            // 將填寫的調撥量依序填入各批號,直到數量都分配完
            if (remainQty > 0) {
                if (remainQty >= T2Store.data.items[i].data['INV_QTY']) {
                    // 若庫存量有值則調撥量填庫存量,若無值(新增項目)則填入剩餘的調撥量
                    if (T2Store.data.items[i].data['INV_QTY']) {
                        T2Store.data.items[i].data['APP_QTY'] = T2Store.data.items[i].data['INV_QTY'];
                        remainQty = remainQty - T2Store.data.items[i].data['INV_QTY'];
                    }
                    else {
                        T2Store.data.items[i].data['APP_QTY'] = remainQty;
                        remainQty = 0;
                    }
                }
                else {
                    T2Store.data.items[i].data['APP_QTY'] = remainQty;
                    remainQty = 0;
                }
                T2Store.loadRecords(T2Store.getRange());
            }
        }
    }

    function T1Load() {
        msglabel('');
        T1Tool.moveFirst();
    }

    function setT2Form(selRec) {
        if (popform.getForm().findField('MMCODE').getValue() == null)
            popform.getForm().findField('MMCODE').setValue(selRec.data.MMCODE);
        popform.getForm().findField('MMNAME_C').setValue(selRec.data.MMNAME_C);
        popform.getForm().findField('MMNAME_E').setValue(selRec.data.MMNAME_E);
        popform.getForm().findField('FRWH').setValue('');
        popform.getForm().findField('FRWH_QTY').setValue('');
        towh_store.load();
        popform.getForm().findField('TOWH').setValue('');
        popform.getForm().findField('TOWH_QTY').setValue('');

        popform.getForm().findField('WEXP_ID').setValue(selRec.data.WEXP_ID);
        // 批號效期管制則顯示維護Grid
        T2Store.removeAll();
        // if (selRec.data.WEXP_ID == 'Y') {
        popform.down('#itemdetailgrid').setVisible(true);
        popform.down('#btn_newExp').setVisible(true);
        popform.down('#btn_delExp').setVisible(true);
        popform.down('#btn_delExp').setDisabled(true);
        //}
        //else {
        //    popform.down('#itemdetailgrid').setVisible(false);
        //    popform.down('#btn_newExp').setVisible(false);
        //    popform.down('#btn_delExp').setVisible(false);
        //}  
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
        }]
    });

    T1Query.getForm().findField('P0').focus();

    getDefaultColumns();
});
