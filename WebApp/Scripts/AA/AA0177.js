Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "一級庫基準量維護";
    var T1Set = '/api/AA0177/Update';
    var T1Rec = 0;
    var T1LastRec = null;
    var p0_default = "";
    var ro_type = "";
    var T1FormStatus = "";
    var btn_mark = "N";

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //物料分類
    var SetMatClassComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AA0177/GetMatClassSubCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P0 = T1Query.getForm().findField('P0');
                if (DataCount > 0) {
                    combo_P0.setValue(store.getAt(0).get('VALUE'));
                    p0_default = store.getAt(0).get('VALUE');
                }
            }
        },
        autoLoad: true
    });
    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0177/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Query.getForm().findField('P0').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                T1Query.getForm().findField('P1').setValue(r.data.MMCODE);
                T1Query.getForm().findField('F1').setValue(r.data.MMNAME_C);
                T1Query.getForm().findField('F2').setValue(r.data.MMNAME_E);
            }
        }
    });
    var uriWH_NoCombo = '/api/AA0177/GetWH_NoCombo';
    var T1QueryWH_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        id: 'P2',
        name: 'P2',
        queryUrl: uriWH_NoCombo,
        fieldLabel: '庫房代碼',
        labelAlign: 'right',
        fieldCls: 'required',
        allowBlank: false,
        listeners: {
            afterrender: function (rec) {
                Ext.Ajax.request({
                    url: uriWH_NoCombo,
                    method: reqVal_p,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            if (data.etts) {
                                Ext.getCmp('P2').setValue(data.etts[0].WH_NO);
                            }
                        }
                    }
                });
            }
        }
    });
    // 查詢欄位
    var mLabelWidth = 80;
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
            width: mWidth,
            margin: '2 4 0 0'
        },
        items: [{
            xtype: 'container',
            layout: 'hbox',
            defaults: {
                xtype: 'displayfield',
                labelWidth: 120,
                width: 220,
                readOnly: true
            },

        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    store: SetMatClassComboGet,
                    fieldLabel: '藥材類別',// 即是 物料分類子類別
                    name: 'P0',
                    id: 'P0',
                    queryMode: 'local',
                    labelAlign: 'right',
                    fieldCls: 'required',
                    limit: 10, //限制一次最多顯示10筆
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    requiredFields: ['VALUE'],
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {
                            T1Query.getForm().findField('P1').setValue("");
                            T1Query.getForm().findField('F1').setValue("");
                            T1Query.getForm().findField('F2').setValue("");

                        }
                    },
                    padding: '0 4 0 4'
                },
                T1QueryMMCode,
                T1QueryWH_NoCombo,
                {
                    xtype: 'button',
                    text: '查詢',
                    margin: '2 4 0 20',
                    handler: function () {
                        if ((this.up('form').getForm().findField('P0').getValue() == '' || this.up('form').getForm().findField('P0').getValue() == null) ||
                            (this.up('form').getForm().findField('P2').getValue() == '' || this.up('form').getForm().findField('P2').getValue() == null)
                        ) {
                            Ext.Msg.alert('提醒', '[物料類別] [庫房代碼]不可空白');
                            return;
                        }
                        T1reset();
                        T1Load();
                        msglabel('訊息區:');
                    }

                }, {
                    xtype: 'button',
                    text: '清除',
                    margin: '2 4 0 0',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        T1Query.getForm().findField('P0').setValue(p0_default);
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        T1Query.getForm().findField('P1').SetValue("");
                        T1Query.getForm().findField('F1').SetValue("");
                        T1Query.getForm().findField('F2').SetValue("");
                        msglabel('訊息區:');
                    }
                }
            ]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'displayfield',
                    fieldLabel: '中文品名',
                    name: 'F1',
                    id: 'F1',
                    width: 300
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '英文品名',
                    name: 'F2',
                    id: 'F2',
                    width: 500
                }
            ]
        }]
    });
    var T1Store = Ext.create('WEBAPP.store.AA.AA0177', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1reset();
                }
                else {
                    if (records.length > 0) {
                        SetFormButton('1');
                        Ext.getCmp('excel').setDisabled(false);
                    }
                    else {
                        msglabel('查無資料!');
                    }
                }
            }
        }
    });
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        displayInfo: true,
        plain: true,
        buttons: [
            {
                text: '修改',
                name: 'btnT1edit',
                id: 'btnT1edit',
                disabled: true,
                handler: function () {
                    msglabel('訊息區:');
                    if (T1LastRec) {
                        setFormEdit();
                    }
                    else {
                        Ext.Msg.alert('提醒', '請先選擇一筆資料');
                    }
                }
            },
            {
                text: '匯出',
                name: 'excel',
                id: 'excel',
                disabled: true,
                handler: function () {
                    if (T1Query.getForm().findField('P0').getValue() == '' || T1Query.getForm().findField('P0').getValue() == null) {
                        Ext.Msg.alert('提醒', '[物料類別]不可空白');
                        return;
                    }
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                            p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                            PostForm('/api/AA0177/GetExcel', p);
                            msglabel('訊息區:匯出完成');
                        }
                    });
                }
            },
            {
                itemId: 'export',
                text: '匯入',
                handler: function () {
                    msglabel('訊息區:');
                    showWin6();
                }
            },
        ]
    });
    var T1Grid = Ext.create('Ext.grid.Panel', {
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
            { xtype: 'rownumberer' },
            { text: "院內碼", dataIndex: 'MMCODE', width: 100 },
            { text: "品名", dataIndex: 'MMNAME_C', width: 200 },
            { text: "出貨單位", dataIndex: 'UNITRATE', width: 100 },
            { text: "轉換量", dataIndex: 'TRUTRATE', width: 80 },
            { text: "類別", dataIndex: 'MAT_CLSNAME', width: 80 },
            { text: "基準量模式", dataIndex: 'RO_TYPE', width: 90 },
            { text: "現用基準量", dataIndex: 'NOW_RO', width: 90 },
            { text: "日平均消耗10天", dataIndex: 'DAY_USE_10', width: 120 },
            { text: "日平均消耗14天", dataIndex: 'DAY_USE_14', width: 120 },
            { text: "日平均消耗90天", dataIndex: 'DAY_USE_90', width: 120 },
            { text: "前第一個月消耗", dataIndex: 'MON_USE_1', width: 120 },
            { text: "前第二個月消耗", dataIndex: 'MON_USE_2', width: 120 },
            { text: "前第三個月消耗", dataIndex: 'MON_USE_3', width: 120 },
            { text: "前第四個月消耗", dataIndex: 'MON_USE_4', width: 120 },
            { text: "前第五個月消耗", dataIndex: 'MON_USE_5', width: 120 },
            { text: "前第六個月消耗", dataIndex: 'MON_USE_6', width: 120 },
            { text: "三個月平均消耗量", dataIndex: 'MON_AVG_USE_3', width: 130 },
            { text: "六個月平均消耗量", dataIndex: 'MON_AVG_USE_6', width: 130 },
            { text: "護理病房最大請領量", dataIndex: 'G34_MAX_APPQTY', width: 140 },
            { text: "供應中心最大請領量", dataIndex: 'SUPPLY_MAX_APPQTY', width: 140 },
            { text: "藥局請領最大量", dataIndex: 'PHR_MAX_APPQTY', width: 120 },
            { text: "戰備存量", dataIndex: 'WAR_QTY', width: 80 },
            { text: "安全庫存量", dataIndex: 'SAFE_QTY', width: 90 },
            { text: "正常庫存量", dataIndex: 'NORMAL_QTY', width: 90 },
            { text: "誤差百分比", dataIndex: 'DIFF_PERC', width: 90 },
            { text: "安全存量比值百分比", dataIndex: 'SAFE_PERC', width: 140 },
            { text: "藥材庫現有存量", dataIndex: 'INV_QTY_1', width: 120 },
            { text: "藥局現有存量", dataIndex: 'INV_QTY_2', width: 100 },
            { text: "供應中心存量", dataIndex: 'INV_QTY_3', width: 100 },
            { text: "安全庫存量RO倍數", dataIndex: 'SAFE_PERC', width: 130 },
            { text: "正常庫存量RO倍數", dataIndex: 'NORMAL_PERC', width: 130 },
            { text: "供應中心最大請領RO倍數", dataIndex: 'SUPPLY_PERC', width: 180 },
            { text: "藥局最大請領RO倍數", dataIndex: 'PHR_PERC', width: 150 },
            { text: "護理病房最大請領RO倍數", dataIndex: 'G34_PERC', width: 180 },
            { text: "戰備存量RO倍數", dataIndex: 'WAR_PERC', width: 130 },
            { text: "上次異動人員", dataIndex: 'UPDATE_USER', width: 100 },
            { text: "上次異動時間", dataIndex: 'UPDATE_TIME', width: 100 },
            { text: "", dataIndex: 'RO_WHTYPE', width: 80, hidden: true },
            { header: "", flex: 1 }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
            },
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                var MMCODE = record.data.MMCODE;
                msglabel('訊息區:');
                Ext.getCmp('btnT1edit').setDisabled(false);
                T1LastRec = Ext.clone(record);
                T1Form.loadRecord(T1LastRec);
                T1PromptForm.loadRecord(T1LastRec);
                setFormDefaultDisplay();
                SetFormButton('1');
                viewport.down('#form').expand();
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    var winRoSet;
    var createWinRoSet = function () {
        winRoSet = Ext.create('widget.window', {
            title: '設定比值',
            layout: 'fit',
            header: {
                titlePosition: 2,
                titleAlign: 'center'
            },
            modal: true,
            closable: true,
            closeAction: 'hide',
            minWidth: 350,
            height: 350,
            items: [
                T1PromptForm
            ],
            buttons: [
                {
                    text: '儲存',
                    handler: function () {
                        if (ro_type == "") {
                            Ext.Msg.alert('提醒', '資料未變更');
                            msglabel('訊息區:資料未變更');
                        }
                        else if (T1PromptForm.getForm().isValid()) { // 檢查T1PromptForm填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                            Ext.MessageBox.confirm('提示', '是否確定儲存?', function (btn, text) {
                                if (btn === 'yes') {
                                    viewport.down('#t1Grid').unmask();
                                    T1UpdatePerc();
                                }
                            });
                        }
                        else {
                            Ext.Msg.alert('提醒', '輸入資料格式有誤');
                            msglabel('訊息區:輸入資料格式有誤');
                        }
                        this.up("window").close();
                    }
                }, {
                    text: '關閉',
                    handler: function () {
                        this.up("window").close();
                    }
                }
            ]
        });
    }
    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        //cls: 'T1b',
        title: '',
        autoScroll: true,
        //bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'left',
            msgTarget: 'side',
            padding: '0 5'
        },
        defaultType: 'textfield',
        items: [{
            xtype: 'container',
            layout: 'form',
            items: [{
                xtype: 'container',
                layout: {
                    type: 'table',
                    columns: 4
                },
                defaults: {
                    xtype: 'button',
                    width: 200,
                    margin: '0 10',
                    disabled: true
                },
                items: [{
                    text: '日基準模式',
                    id: 'ro_type_button_1',
                    name: 'ro_type_button_1',
                    listeners: {
                        click: function (field, newValue, oldValue) {
                            CalcRoType_1();
                        }
                    }
                }, {
                    text: '月基準模式',
                    id: 'ro_type_button_2',
                    name: 'ro_type_button_2',
                    listeners: {
                        click: function (field, newValue, oldValue) {
                            CalcRoType_2();
                        }
                    }
                }, {
                    text: '自訂基準模式',
                    id: 'ro_type_button_3',
                    name: 'ro_type_button_3',
                    listeners: {
                        click: function (field, newValue, oldValue) {
                            CalcRoType_3();
                        }
                    }
                }, {
                    text: '設定比值',
                    id: 'ro_type_button_4',
                    name: 'ro_type_button_4',
                    listeners: {
                        click: function (field, newValue, oldValue) {
                            if (!winRoSet) {
                                createWinRoSet();
                            }
                            if (winRoSet.isVisible()) {
                                winRoSet.hide();
                            } else {
                                winRoSet.show();
                            }
                            CalcRoType_4();
                        }
                    }
                }]
            }]
        }, {
            xtype: 'container',
            layout: 'form',
            items: [{
                xtype: 'container',
                layout: 'hbox',
                defaults: {
                    xtype: 'displayfield',
                    labelWidth: 120,
                    width: 220,
                    readOnly: true
                },
                items: [{
                    fieldLabel: '基準量模式',
                    name: 'RO_TYPE'
                }]
            }, {
                xtype: 'container',
                layout: 'hbox',
                items: [{
                    xtype: 'container',
                    layout: 'vbox',
                    defaults: {
                        xtype: 'displayfield',
                        labelWidth: 120,
                        width: 220,
                        readOnly: true
                    },
                    items: [{
                        fieldLabel: '日平均消耗10天',
                        name: 'DAY_USE_10'
                    }, {
                        fieldLabel: '日平均消耗14天',
                        name: 'DAY_USE_14'
                    }, {
                        fieldLabel: '日平均消耗90天',
                        name: 'DAY_USE_90'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '前第一個月消耗',
                        name: 'MON_USE_1'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '前第二個月消耗',
                        name: 'MON_USE_2'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '前第三個月消耗',
                        name: 'MON_USE_3'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '前第四個月消耗',
                        name: 'MON_USE_4'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '前第五個月消耗',
                        name: 'MON_USE_5'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '前第六個月消耗',
                        name: 'MON_USE_6'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '三個月平均',
                        name: 'MON_AVG_USE_3'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '六個月平均',
                        name: 'MON_AVG_USE_6'
                    }]
                }, {
                    xtype: 'container',
                    layout: {
                        type: 'table',
                        trAttrs: {
                            height: 52
                        },
                        columns: 1,
                        margin: 0,
                        padding: 0
                    },
                    defaults: {
                        labelWidth: 110,
                        width: 220,
                        readOnly: true,
                        enforceMaxLength: true,
                        maxLength: 10
                    },
                    items: [
                        {
                            xtype: 'numberfield',
                            fieldLabel: '基準量(X1RO)',
                            name: 'NOW_RO',
                            fieldCls: 'required',
                            allowBlank: false,
                            minValue: 0,
                            submitValue: true,
                            listeners: {
                                change: function (field, newValue, oldValue) {
                                    if (T1FormStatus == "edit" && btn_mark == "N") {
                                        ro_type = "3";
                                        T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                        CalB();
                                    }
                                }
                            }
                        }, {
                            xtype: 'numberfield',
                            fieldLabel: '設定90天及10天誤差百分比(%)',
                            name: 'DIFF_PERC',
                            fieldCls: 'required',
                            allowBlank: false,
                            minValue: 0,
                            submitValue: true,
                            listeners: {
                                change: function (field, newValue, oldValue) {
                                    if (T1FormStatus == "edit" && btn_mark == "N") {
                                        ro_type = "3";
                                        T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    }
                                }
                            }
                        }, {
                            xtype: 'displayfield',
                            fieldLabel: '原訂日基準量',
                            name: 'OLD_DAY_RO'
                        }, {
                            xtype: 'numberfield',
                            fieldLabel: '新訂日基準量',
                            name: 'DAY_RO',
                            fieldCls: 'required',
                            allowBlank: false,
                            minValue: 0,
                            submitValue: true,
                            listeners: {
                                change: function (field, newValue, oldValue) {
                                    if (T1FormStatus == "edit" && btn_mark == "N") {
                                        ro_type = "3";
                                        T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    }
                                }
                            }
                        }, {
                            xtype: 'displayfield'
                        }, {
                            xtype: 'displayfield',
                            fieldLabel: '原訂基準量',
                            name: 'OLD_NOW_RO'
                        }]
                }, {
                    xtype: 'container',
                    width: 50,
                    layout: {
                        type: 'table',
                        trAttrs: {
                            height: 52
                        },
                        columns: 1,
                        margin: 0,
                        padding: 0
                    },
                    defaults: {
                        xtype: 'button',
                        disabled: true
                    },
                    items: [
                        {
                            xtype: 'displayfield'
                        }, {
                            text: '初值',
                            name: 'diff_perc_button',
                            id: 'diff_perc_button',
                            listeners: {
                                click: function (field, newValue, oldValue) {
                                    ro_type = "3";
                                    T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    T1Form.getForm().findField("DIFF_PERC").setValue(T1LastRec.data.DIFF_PERC);
                                }
                            }
                        }, {
                            xtype: 'displayfield'
                        }, {
                            text: '初值',
                            name: 'day_ro_button',
                            id: 'day_ro_button',
                            listeners: {
                                click: function (field, newValue, oldValue) {
                                    ro_type = "3";
                                    T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    T1Form.getForm().findField("DAY_RO").setValue(T1LastRec.data.DAY_RO);
                                }
                            }
                        }, {
                            xtype: 'displayfield'
                        }, {
                            xtype: 'displayfield'
                        }]
                }, {
                    xtype: 'container',
                    layout: {
                        type: 'table',
                        trAttrs: {
                            height: 52
                        },
                        columns: 1,
                        margin: 0,
                        padding: 0
                    },
                    defaults: {
                        labelWidth: 110,
                        width: 220,
                        readOnly: true,
                        enforceMaxLength: true,
                        maxLength: 10
                    },
                    items: [
                        {
                            xtype: 'displayfield'
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '藥材庫安全庫存量(X0.5RO)',
                            name: 'SAFE_QTY',
                            fieldCls: 'required',
                            allowBlank: false,
                            minValue: 0,
                            submitValue: true,
                            listeners: {
                                change: function () {
                                    if (T1FormStatus == "edit" && btn_mark == "N") {
                                        ro_type = "3";
                                        T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    }
                                }
                            }
                        }, {
                            xtype: 'numberfield',
                            fieldLabel: '藥材庫正常庫存量(X1RO)',
                            name: 'NORMAL_QTY',
                            fieldCls: 'required',
                            allowBlank: false,
                            minValue: 0,
                            submitValue: true,
                            listeners: {
                                change: function () {
                                    if (T1FormStatus == "edit" && btn_mark == "N") {
                                        ro_type = "3";
                                        T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    }
                                }
                            }
                        }, {
                            xtype: 'numberfield',
                            fieldLabel: '供應中心最大請領量(X0.8RO)',
                            name: 'SUPPLY_MAX_APPQTY',
                            fieldCls: 'required',
                            allowBlank: false,
                            minValue: 0,
                            submitValue: true,
                            listeners: {
                                change: function () {
                                    if (T1FormStatus == "edit" && btn_mark == "N") {
                                        ro_type = "3";
                                        T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    }
                                }
                            }
                        }, {
                            xtype: 'numberfield',
                            fieldLabel: '藥局最大請領量(X0.5RO)',
                            name: 'PHR_MAX_APPQTY',
                            fieldCls: 'required',
                            allowBlank: false,
                            minValue: 0,
                            submitValue: true,
                            listeners: {
                                change: function () {
                                    if (T1FormStatus == "edit" && btn_mark == "N") {
                                        ro_type = "3";
                                        T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    }
                                }
                            }
                        }, {
                            xtype: 'numberfield',
                            fieldLabel: '護理病房及檢驗科最大請領量(X0.25RO)',
                            name: 'G34_MAX_APPQTY',
                            fieldCls: 'required',
                            allowBlank: false,
                            minValue: 0,
                            submitValue: true,
                            listeners: {
                                change: function () {
                                    if (T1FormStatus == "edit" && btn_mark == "N") {
                                        ro_type = "3";
                                        T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    }
                                }
                            }
                        }, {
                            xtype: 'numberfield',
                            fieldLabel: '戰備存量(X2RO)',
                            name: 'WAR_QTY',
                            fieldCls: 'required',
                            allowBlank: false,
                            minValue: 0,
                            submitValue: true,
                            listeners: {
                                change: function () {
                                    if (T1FormStatus == "edit" && btn_mark == "N") {
                                        ro_type = "3";
                                        T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    }
                                }
                            }
                        }]
                }, {
                    xtype: 'container',
                    width: 50,
                    layout: {
                        type: 'table',
                        trAttrs: {
                            height: 52
                        },
                        columns: 1,
                        margin: 0,
                        padding: 0
                    },
                    defaults: {
                        xtype: 'button',
                        disabled: true
                    },
                    items: [
                        {
                            xtype: 'displayfield'
                        }, {
                            text: '初值',
                            name: 'safe_qty_button',
                            id: 'safe_qty_button',
                            listeners: {
                                click: function (field, newValue, oldValue) {
                                    ro_type = "3";
                                    T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    T1Form.getForm().findField("SAFE_QTY").setValue(T1LastRec.data.SAFE_QTY);
                                }
                            }
                        }, {
                            text: '初值',
                            name: 'normal_qty_button',
                            id: 'normal_qty_button',
                            listeners: {
                                click: function (field, newValue, oldValue) {
                                    ro_type = "3";
                                    T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    T1Form.getForm().findField("NORMAL_QTY").setValue(T1LastRec.data.NORMAL_QTY);
                                }
                            }
                        }, {
                            text: '初值',
                            name: 'supply_max_appqty_button',
                            id: 'supply_max_appqty_button',
                            listeners: {
                                click: function (field, newValue, oldValue) {
                                    ro_type = "3";
                                    T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    T1Form.getForm().findField("SUPPLY_MAX_APPQTY").setValue(T1LastRec.data.SUPPLY_MAX_APPQTY);
                                }
                            }
                        }, {
                            text: '初值',
                            name: 'phr_max_appqty_button',
                            id: 'phr_max_appqty_button',
                            listeners: {
                                click: function (field, newValue, oldValue) {
                                    ro_type = "3";
                                    T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    T1Form.getForm().findField("PHR_MAX_APPQTY").setValue(T1LastRec.data.PHR_MAX_APPQTY);
                                }
                            }
                        }, {
                            text: '初值',
                            name: 'g34_max_appqty_button',
                            id: 'g34_max_appqty_button',
                            listeners: {
                                click: function (field, newValue, oldValue) {
                                    ro_type = "3";
                                    T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    T1Form.getForm().findField("G34_MAX_APPQTY").setValue(T1LastRec.data.G34_MAX_APPQTY);
                                }
                            }
                        }, {
                            text: '初值',
                            name: 'war_qty_button',
                            id: 'war_qty_button',
                            listeners: {
                                click: function (field, newValue, oldValue) {
                                    ro_type = "3";
                                    T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
                                    T1Form.getForm().findField("WAR_QTY").setValue(T1LastRec.data.WAR_QTY);
                                }
                            }
                        }]
                }]
            }]
        }, {
            xtype: 'container',
            layout: 'form',
            items: [{
                xtype: 'container',
                layout: {
                    type: 'table',
                    columns: 3
                },
                defaults: {
                    xtype: 'displayfield',
                    labelWidth: 120,
                    width: 220,
                    readOnly: true
                },
                items: [{
                    fieldLabel: '藥局現有存量',
                    name: 'INV_QTY_2'
                }, {
                    fieldLabel: '藥庫現有存量',
                    name: 'INV_QTY_1'
                }, {
                    fieldLabel: '供應中心現有存量',
                    name: 'INV_QTY_3'
                }]
            }]
        }, {
            xtype: 'container',
            layout: 'form',
            items: [{
                xtype: 'container',
                layout: {
                    type: 'table',
                    columns: 3
                },
                defaults: {
                    xtype: 'displayfield',
                    labelWidth: 120,
                    width: 230,
                    readOnly: true
                },
                items: [{
                    fieldLabel: '異動人員',
                    name: 'UPDATE_USER'
                }, {
                    fieldLabel: '異動時間',
                    name: 'UPDATE_TIME'
                }]
            }]
        }],
        buttons: [
            {
                id: 'submit', itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (ro_type == "") {
                        Ext.Msg.alert('提醒', '資料未變更');
                        msglabel('訊息區:資料未變更');
                    }
                    else if (T1Form.getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        Ext.MessageBox.confirm('提示', '是否確定儲存?', function (btn, text) {
                            if (btn === 'yes') {
                                viewport.down('#t1Grid').unmask();
                                T1Update();
                            }
                        });
                    }
                    else {
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                        msglabel('訊息區:輸入資料格式有誤');
                    }
                }
            }, {
                id: 'cancel', itemId: 'cancel', text: '取消', hidden: true, handler: function () {
                    T1Form.loadRecord(T1LastRec);
                    SetFormButton('1');
                    T1FormStatus = "";
                    ro_type = "";
                    T1Form.getForm().findField("RO_TYPE").setValue(T1LastRec.data.RO_TYPE);
                    viewport.down('#t1Grid').unmask();
                }
            }
        ]
    });
    // 設定比值 彈出視窗
    var T1PromptForm = Ext.create('Ext.form.Panel', {
        layout: 'anchor',
        defaults: {
            anchor: '100%'
        },
        defaultType: 'numberfield',
        fieldDefaults: {
            labelWidth: 80,
            labelAlign: "left",
            flex: 1,
            margin: 5
        },
        items: [{
            fieldLabel: '安全庫存量RO倍數',
            name: 'SAFE_PERC',
            allowBlank: false,
            minValue: 0
        }, {
            fieldLabel: '正常庫存量RO倍數',
            name: 'NORMAL_PERC',
            allowBlank: false,
            minValue: 0
        }, {
            fieldLabel: '供應中心最大請領RO倍數',
            name: 'SUPPLY_PERC',
            allowBlank: false,
            minValue: 0
        }, {
            fieldLabel: '藥局最大請領RO倍數',
            name: 'PHR_PERC',
            allowBlank: false,
            minValue: 0
        }, {
            fieldLabel: '護理病房最大請領RO倍數',
            name: 'G34_PERC',
            allowBlank: false,
            minValue: 0
        }, {
            fieldLabel: '戰備存量比值',
            name: 'WAR_PERC',
            allowBlank: false,
            minValue: 0
        }],
    });

    //還原初始設置
    function T1reset() {
        Ext.getCmp('btnT1edit').setDisabled(true);
        Ext.getCmp('excel').setDisabled(true);
        SetFormButton('1');
        T1FormStatus = "";
        ro_type = "";
        T1LastRec = null;
        T1Form.getForm().reset();
        viewport.down('#form').collapse();
        T1Store.removeAll();
        msglabel('訊息區:');
    }
    //控制右側form按鈕
    function SetFormButton(param) {
        if (param == "1") {//按鈕全部隱藏
            T1Form.getForm().findField('NOW_RO').setReadOnly(true);
            T1Form.getForm().findField('DIFF_PERC').setReadOnly(true);
            T1Form.getForm().findField('DAY_RO').setReadOnly(true);
            T1Form.getForm().findField('SAFE_QTY').setReadOnly(true);
            T1Form.getForm().findField('NORMAL_QTY').setReadOnly(true);
            T1Form.getForm().findField('SUPPLY_MAX_APPQTY').setReadOnly(true);
            T1Form.getForm().findField('PHR_MAX_APPQTY').setReadOnly(true);
            T1Form.getForm().findField('G34_MAX_APPQTY').setReadOnly(true);
            T1Form.getForm().findField('WAR_QTY').setReadOnly(true);
            Ext.getCmp('diff_perc_button').setDisabled(true);
            Ext.getCmp('day_ro_button').setDisabled(true);
            Ext.getCmp('safe_qty_button').setDisabled(true);
            Ext.getCmp('normal_qty_button').setDisabled(true);
            Ext.getCmp('supply_max_appqty_button').setDisabled(true);
            Ext.getCmp('phr_max_appqty_button').setDisabled(true);
            Ext.getCmp('g34_max_appqty_button').setDisabled(true);
            Ext.getCmp('war_qty_button').setDisabled(true);
            Ext.getCmp('ro_type_button_1').setDisabled(true);
            Ext.getCmp('ro_type_button_2').setDisabled(true);
            Ext.getCmp('ro_type_button_3').setDisabled(true);
            Ext.getCmp('ro_type_button_4').setDisabled(true);
            Ext.getCmp('submit').hide();
            Ext.getCmp('cancel').hide();
        }
        else if (param == "2") {//按鈕全部開啟
            T1Form.getForm().findField('NOW_RO').setReadOnly(false);
            T1Form.getForm().findField('DIFF_PERC').setReadOnly(false);
            T1Form.getForm().findField('DAY_RO').setReadOnly(false);
            T1Form.getForm().findField('SAFE_QTY').setReadOnly(false);
            T1Form.getForm().findField('NORMAL_QTY').setReadOnly(false);
            T1Form.getForm().findField('SUPPLY_MAX_APPQTY').setReadOnly(false);
            T1Form.getForm().findField('PHR_MAX_APPQTY').setReadOnly(false);
            T1Form.getForm().findField('G34_MAX_APPQTY').setReadOnly(false);
            T1Form.getForm().findField('WAR_QTY').setReadOnly(false);
            Ext.getCmp('diff_perc_button').setDisabled(false);
            Ext.getCmp('day_ro_button').setDisabled(false);
            Ext.getCmp('safe_qty_button').setDisabled(false);
            Ext.getCmp('normal_qty_button').setDisabled(false);
            Ext.getCmp('supply_max_appqty_button').setDisabled(false);
            Ext.getCmp('phr_max_appqty_button').setDisabled(false);
            Ext.getCmp('g34_max_appqty_button').setDisabled(false);
            Ext.getCmp('war_qty_button').setDisabled(false);
            Ext.getCmp('ro_type_button_1').setDisabled(false);
            Ext.getCmp('ro_type_button_2').setDisabled(false);
            Ext.getCmp('ro_type_button_3').setDisabled(false);
            Ext.getCmp('ro_type_button_4').setDisabled(false);
            Ext.getCmp('submit').show();
            Ext.getCmp('cancel').show();
        }
    }
    //初值顯示
    function setFormDefaultDisplay() {
        T1Form.getForm().findField("OLD_DAY_RO").setValue(T1LastRec.data.DAY_RO);
        T1Form.getForm().findField("OLD_NOW_RO").setValue(T1LastRec.data.NOW_RO);
        // dynamic change field text
        T1Form.getForm().findField("SAFE_QTY").labelEl.update('藥材庫安全庫存量(X' + T1LastRec.data.SAFE_PERC + 'RO)');
        T1Form.getForm().findField("NORMAL_QTY").labelEl.update('藥材庫正常庫存量(X' + T1LastRec.data.NORMAL_PERC + 'RO)');
        T1Form.getForm().findField("SUPPLY_MAX_APPQTY").labelEl.update('供應中心最大請領量(X' + T1LastRec.data.SUPPLY_PERC + 'RO)');
        T1Form.getForm().findField("PHR_MAX_APPQTY").labelEl.update('藥局最大請領量(X' + T1LastRec.data.PHR_PERC + 'RO)');
        T1Form.getForm().findField("G34_MAX_APPQTY").labelEl.update('護理病房及檢驗科最大請領量(X' + T1LastRec.data.G34_PERC + 'RO)');
        T1Form.getForm().findField("WAR_QTY").labelEl.update('戰備存量(X' + T1LastRec.data.WAR_PERC + 'RO)');
    }
    //按下修改按鈕
    function setFormEdit() {
        viewport.down('#t1Grid').mask();
        if (T1LastRec) {
            viewport.down('#form').expand();
        }
        T1FormStatus = "edit";
        ro_type = "";
        var f = T1Form.getForm();
        SetFormButton('2');
    }
    //計算日基準模式
    function CalcRoType_1() {
        btn_mark = "Y";
        var DAY_USE_90 = T1Form.getForm().findField("DAY_USE_90").getValue();
        var DAY_USE_10 = T1Form.getForm().findField("DAY_USE_10").getValue();
        var DIFF_PERC = T1Form.getForm().findField("DIFF_PERC").getValue();
        var gap = Math.abs(DAY_USE_90 - DAY_USE_10);//90天-10天的絕對值
        var base = Math.ceil(DAY_USE_90 * (DIFF_PERC / 100));
        if (gap >= base) {
            T1Form.getForm().findField("NOW_RO").setValue(Math.ceil(DAY_USE_10 / 10 * 90));
        }
        else {
            T1Form.getForm().findField("NOW_RO").setValue(Math.ceil(DAY_USE_90 * 90));
        }
        T1Form.getForm().findField("RO_TYPE").setValue('日基準');
        CalB();
        ro_type = "1";
        btn_mark = "N";
    }
    //計算月基準模式
    function CalcRoType_2() {
        btn_mark = "Y";
        var MON_AVG_USE_3 = T1Form.getForm().findField("MON_AVG_USE_3").getValue();
        T1Form.getForm().findField("RO_TYPE").setValue('月基準');
        T1Form.getForm().findField("NOW_RO").setValue(MON_AVG_USE_3);
        CalB();
        ro_type = "2";
        btn_mark = "N";
    }
    //計算自訂基準模式
    function CalcRoType_3() {
        ro_type = "3";
        T1Form.getForm().findField("RO_TYPE").setValue('自訂基準');
        T1Form.getForm().findField("NOW_RO").setValue(T1LastRec.data.NOW_RO);
        T1Form.getForm().findField("DIFF_PERC").setValue(T1LastRec.data.DIFF_PERC);
        T1Form.getForm().findField("DAY_RO").setValue(T1LastRec.data.DAY_RO);
        T1Form.getForm().findField("SAFE_QTY").setValue(T1LastRec.data.SAFE_QTY);
        T1Form.getForm().findField("NORMAL_QTY").setValue(T1LastRec.data.NORMAL_QTY);
        T1Form.getForm().findField("SUPPLY_MAX_APPQTY").setValue(T1LastRec.data.SUPPLY_MAX_APPQTY);
        T1Form.getForm().findField("PHR_MAX_APPQTY").setValue(T1LastRec.data.PHR_MAX_APPQTY);
        T1Form.getForm().findField("G34_MAX_APPQTY").setValue(T1LastRec.data.G34_MAX_APPQTY);
        T1Form.getForm().findField("WAR_QTY").setValue(T1LastRec.data.WAR_QTY);
        //CalB();
    }
    function CalcRoType_4() {
        ro_type = "4";
    }
    //依數值統一計算
    function CalB() {
        var NOW_RO = T1Form.getForm().findField("NOW_RO").getValue();
        var SAFE_PERC = T1PromptForm.getForm().findField("SAFE_PERC").getValue();
        var NORMAL_PERC = T1PromptForm.getForm().findField("NORMAL_PERC").getValue();
        var SUPPLY_PERC = T1PromptForm.getForm().findField("SUPPLY_PERC").getValue();
        var G34_PERC = T1PromptForm.getForm().findField("G34_PERC").getValue();
        var PHR_PERC = T1PromptForm.getForm().findField("PHR_PERC").getValue();
        var WAR_PERC = T1PromptForm.getForm().findField("WAR_PERC").getValue();

        T1Form.getForm().findField("SAFE_QTY").setValue(Math.ceil(SAFE_PERC * NOW_RO));
        T1Form.getForm().findField("NORMAL_QTY").setValue(Math.ceil(NORMAL_PERC * NOW_RO));
        T1Form.getForm().findField("SUPPLY_MAX_APPQTY").setValue(Math.ceil(SUPPLY_PERC * NOW_RO));
        T1Form.getForm().findField("PHR_MAX_APPQTY").setValue(Math.ceil(PHR_PERC * NOW_RO));
        T1Form.getForm().findField("G34_MAX_APPQTY").setValue(Math.ceil(G34_PERC * NOW_RO));
        T1Form.getForm().findField("WAR_QTY").setValue(Math.ceil(WAR_PERC * NOW_RO));
    }

    function T1Update() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: T1Set,
            method: reqVal_p,
            params: {
                MMCODE: T1LastRec.data.MMCODE,
                NOW_RO: T1Form.getForm().findField("NOW_RO").getValue(),
                DIFF_PERC: T1Form.getForm().findField("DIFF_PERC").getValue(),
                DAY_RO: T1Form.getForm().findField("DAY_RO").getValue(),
                SAFE_QTY: T1Form.getForm().findField("SAFE_QTY").getValue(),
                NORMAL_QTY: T1Form.getForm().findField("NORMAL_QTY").getValue(),
                SUPPLY_MAX_APPQTY: T1Form.getForm().findField("SUPPLY_MAX_APPQTY").getValue(),
                PHR_MAX_APPQTY: T1Form.getForm().findField("PHR_MAX_APPQTY").getValue(),
                G34_MAX_APPQTY: T1Form.getForm().findField("G34_MAX_APPQTY").getValue(),
                WAR_QTY: T1Form.getForm().findField("WAR_QTY").getValue(),
                WH_NO: Ext.getCmp("P2").getValue(),
                RO_TYPE: ro_type
            },
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    Ext.Msg.alert('', '資料更新成功');
                    T1reset();
                    T1Load();
                }
                else {
                    myMask.hide();
                    Ext.Msg.alert('', '資料更新失敗');
                }
            },
            failure: function (response) {
                myMask.hide();
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
    }

    // 更新基準值比例
    function T1UpdatePerc() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        var NOW_RO = T1LastRec.data.NOW_RO;
        var SAFE_PERC = T1PromptForm.getForm().findField("SAFE_PERC").getValue();
        var NORMAL_PERC = T1PromptForm.getForm().findField("NORMAL_PERC").getValue();
        var SUPPLY_PERC = T1PromptForm.getForm().findField("SUPPLY_PERC").getValue();
        var PHR_PERC = T1PromptForm.getForm().findField("PHR_PERC").getValue();
        var G34_PERC = T1PromptForm.getForm().findField("G34_PERC").getValue();
        var WAR_PERC = T1PromptForm.getForm().findField("WAR_PERC").getValue();
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AA0177/UpdatePerc',
            method: reqVal_p,
            params: {
                SAFE_PERC: SAFE_PERC,
                NORMAL_PERC: NORMAL_PERC,
                SUPPLY_PERC: SUPPLY_PERC,
                PHR_PERC: PHR_PERC,
                G34_PERC: G34_PERC,
                WAR_PERC: WAR_PERC,
                SAFE_QTY: Math.ceil(SAFE_PERC * NOW_RO / 100),
                NORMAL_QTY: Math.ceil(NORMAL_PERC * NOW_RO / 100),
                SUPPLY_MAX_APPQTY: Math.ceil(SUPPLY_PERC * NOW_RO / 100),
                PHR_MAX_APPQTY: Math.ceil(PHR_PERC * NOW_RO / 100),
                G34_MAX_APPQTY: Math.ceil(G34_PERC * NOW_RO / 100),
                WAR_QTY: Math.ceil(WAR_PERC * NOW_RO / 100),

                MMCODE: T1LastRec.data.MMCODE,
                WH_NO: T1LastRec.data.WH_NO,
                RO_TYPE: ro_type
            },
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    Ext.Msg.alert('', '資料更新成功');
                    T1reset();
                    T1Load();
                }
                else {
                    myMask.hide();
                    Ext.Msg.alert('', '資料更新失敗');
                }
            },
            failure: function (response) {
                myMask.hide();
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
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
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: '60%',
            title: '瀏覽',
            border: false,
            collapsed: true,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form]
        }]
    });

    Ext.define('T6Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
            { name: 'RO_WHTYPE', type: 'string' },
            { name: 'RO_TYPE', type: 'string' },
            { name: 'NOW_RO', type: 'string' },
            //{ name: 'DAY_USE_10', type: 'string' },
            //{ name: 'DAY_USE_14', type: 'string' },
            //{ name: 'DAY_USE_90', type: 'string' },
            //{ name: 'MON_USE_1', type: 'string' },
            //{ name: 'MON_USE_2', type: 'string' },
            //{ name: 'MON_USE_3', type: 'string' },
            //{ name: 'MON_USE_4', type: 'string' },
            //{ name: 'MON_USE_5', type: 'string' },
            //{ name: 'MON_USE_6', type: 'string' },
            //{ name: 'MON_AVG_USE_3', type: 'string' },
            //{ name: 'MON_AVG_USE_6', type: 'string' },
            { name: 'G34_MAX_APPQTY', type: 'string' },
            { name: 'SUPPLY_MAX_APPQTY', type: 'string' },
            { name: 'PHR_MAX_APPQTY', type: 'string' },
            { name: 'WAR_QTY', type: 'string' },
            { name: 'SAFE_QTY', type: 'string' },
            { name: 'NORMAL_QTY', type: 'string' },
            { name: 'DIFF_PERC', type: 'string' },
            { name: 'SAFE_PERC', type: 'string' },
            { name: 'DAY_RO', type: 'string' },
            { name: 'MON_RO', type: 'string' },
            { name: 'G34_PERC', type: 'string' },
            { name: 'SUPPLY_PERC', type: 'string' },
            { name: 'PHR_PERC', type: 'string' },
            { name: 'NORMAL_PERC', type: 'string' },
            { name: 'WAR_PERC', type: 'string' },
            { name: 'WH_NO', type: 'string' }
        ]
    });

    var T6Store = Ext.create('Ext.data.Store', {
        model: 'T6Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            //url: '/api/AA0159/???',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    //p2: T1F2,
                    //p3: T1F5
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    })

    var T6Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [
            {
                xtype: 'button',
                id: 'T6sample',
                name: 'T6sample',
                text: '範本', handler: function () {
                    var p = new Array();
                    PostForm('/api/AA0177/GetExcelExample', p);
                    msglabel('訊息區:匯出完成');
                }
            },
            {
                xtype: 'filefield',
                name: 'T6send',
                id: 'T6send',
                buttonOnly: true,
                buttonText: '匯入',
                width: 40,
                listeners: {
                    change: function (widget, value, eOpts) {
                        //Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        Ext.getCmp('T6insert').setDisabled(true);
                        T6Store.removeAll();
                        var files = event.target.files;
                        var self = this; // the controller
                        if (!files || files.length == 0) return; // make sure we got something
                        var f = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '請選擇xlsx或xls檔案！');
                            Ext.getCmp('T6send').fileInputEl.dom.value = '';
                            msglabel("請選擇xlsx或xls檔案！");
                        }
                        else {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", f);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AA0177/SendExcel",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    if (!data.success) {
                                        T6Store.removeAll();
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('T6insert').setDisabled(true);
                                        IsSend = false;
                                    }
                                    else {
                                        msglabel("訊息區:檔案讀取成功");
                                        T6Store.loadData(data.etts, false);
                                        IsSend = true;
                                        T6Grid.columns[1].setVisible(true);
                                        //T1Grid.columns[2].setVisible(true);
                                        if (data.msg == "True") {
                                            Ext.getCmp('T6insert').setDisabled(false);
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: blue; font-weight: bold\">成功</span>，可進行更新動作。");
                                        };
                                        if (data.msg == "False") {
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: red; font-weight: bold\">失敗</span>，請依錯誤說明修改Excel檔。");
                                        };
                                    }
                                    Ext.getCmp('T6send').fileInputEl.dom.value = '';
                                    myMask.hide();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    Ext.getCmp('T6send').fileInputEl.dom.value = '';
                                    Ext.getCmp('T6insert').setDisabled(true);
                                    myMask.hide();

                                }
                            });
                        }
                    }
                }
            },
            {
                xtype: 'button',
                text: '更新',
                id: 'T6insert',
                name: 'T6insert',
                disabled: true,
                handler: function () {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    Ext.Ajax.request({
                        url: '/api/AA0177/InsertFromXls',
                        method: reqVal_p,
                        params: {
                            P2: Ext.getCmp('P2').getValue(),
                            data: Ext.encode(Ext.pluck(T6Store.data.items, 'data'))
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                if (data.msg == "True") {
                                    Ext.MessageBox.alert("提示", "<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                    msglabel("訊息區:<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                }
                                else {
                                    Ext.MessageBox.alert("提示", "匯入<span style=\"color: blue; font-weight: bold\">完成</span>。");
                                    msglabel("訊息區:匯入<span style=\"color: red; font-weight: bold\">完成</span>");
                                }
                                Ext.getCmp('T6insert').setDisabled(true);
                                T6Store.removeAll();
                                T6Grid.columns[1].setVisible(false);
                                T1Load();
                            }
                            myMask.hide();
                            Ext.getCmp('T6insert').setDisabled(true);
                        },
                        failure: function (form, action) {
                            myMask.hide();
                            Ext.getCmp('T6insert').setDisabled(true);
                            switch (action.failureType) {
                                case Ext.form.action.Action.CLIENT_INVALID:
                                    Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                    break;
                                case Ext.form.action.Action.CONNECT_FAILURE:
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    break;
                                case Ext.form.action.Action.SERVER_INVALID:
                                    Ext.Msg.alert('失敗', "匯入失敗");
                                    break;
                            }
                        }
                    });

                    hideWin6();
                }
            }
        ]
    });
    var T6Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T6Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T6Query]
            }
        ],
        columns: [
            { xtype: 'rownumberer' },
            {
                text: "檢核結果",
                dataIndex: 'CHECK_RESULT',
                hidden: true,
                width: 250
            },
            { text: "院內碼", dataIndex: 'MMCODE', width: 100 },
            //{ text: "庫房代碼", dataIndex: 'WH_NO', width: 100 },
            { text: "基準量模式", dataIndex: 'RO_TYPE', width: 100 },
            { text: "現用基準量", dataIndex: 'NOW_RO', width: 100 },
            //{ text: "日平均消耗10天", dataIndex: 'DAY_USE_10', width: 100 },
            //{ text: "日平均消耗14天", dataIndex: 'DAY_USE_14', width: 100 },
            //{ text: "日平均消耗90天", dataIndex: 'DAY_USE_90', width: 100 },
            //{ text: "前第1個月消耗", dataIndex: 'MON_USE_1', width: 100 },
            //{ text: "前第2個月消耗", dataIndex: 'MON_USE_2', width: 100 },
            //{ text: "前第3個月消耗", dataIndex: 'MON_USE_3', width: 100 },
            //{ text: "前第4個月消耗", dataIndex: 'MON_USE_4', width: 100 },
            //{ text: "前第5個月消耗", dataIndex: 'MON_USE_5', width: 100 },
            //{ text: "前第6個月消耗", dataIndex: 'MON_USE_6', width: 100 },
            //{ text: "3月平均消耗量", dataIndex: 'MON_AVG_USE_3', width: 100 },
            //{ text: "6月平均消耗量", dataIndex: 'MON_AVG_USE_6', width: 100 },
            { text: "護理病房最大請領量", dataIndex: 'G34_MAX_APPQTY', width: 100 },
            { text: "供應中心最大請領量", dataIndex: 'SUPPLY_MAX_APPQTY', width: 100 },
            { text: "藥局請領最大量", dataIndex: 'PHR_MAX_APPQTY', width: 100 },
            { text: "戰備存量", dataIndex: 'WAR_QTY', width: 100 },
            { text: "安全庫存量", dataIndex: 'SAFE_QTY', width: 100 },
            { text: "正常庫存量", dataIndex: 'NORMAL_QTY', width: 100 },
            { text: "誤差百分比", dataIndex: 'DIFF_PERC', width: 100 },
            { text: "安全存量比值百分比", dataIndex: 'SAFE_PERC', width: 100 },
            //{ text: "日基準量", dataIndex: 'DAY_RO', width: 100 },
            //{ text: "月基準量", dataIndex: 'MON_RO', width: 100 },
            //{ text: "護理病房最大請領RO比值", dataIndex: 'G34_PERC', width: 100 },
            //{ text: "供應中心最大請領RO比值", dataIndex: 'SUPPLY_PERC', width: 100 },
            //{ text: "藥局最大請領RO比值", dataIndex: 'PHR_PERC', width: 100 },
            //{ text: "正常存量RO比值", dataIndex: 'NORMAL_PERC', width: 100 },
            //{ text: "戰備存量RO比值", dataIndex: 'WAR_PERC', width: 100 },
            { header: "", flex: 1 }
        ]
    });

    var winActWidth = viewport.width - 20;
    var winActHeight = viewport.height - 20;
    var win6;
    if (!win6) {
        win6 = Ext.widget('window', {
            title: '匯入',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T6Grid],
            buttons: [{
                text: '關閉',
                handler: function () {
                    hideWin6();
                }
            }],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    }

    function showWin6() {
        if (win6.hidden) {
            T6Cleanup();
            T6Store.removeAll();
            win6.show();
        }
    }
    function hideWin6() {
        if (!win6.hidden) {
            win6.hide();
            T6Cleanup();
        }
    }
    function T6Cleanup() {
        T6Query.getForm().reset();
        msglabel('訊息區:');
    }
});
