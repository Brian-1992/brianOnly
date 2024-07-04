Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };

    var menuLink = Ext.getUrlParam('menuLink');

    var MatClassComboGet = '../../../api/BD0022/GetMatclassCombo';
    var T1GetExcel = '../../../api/BD0022/Excel';
    var reportUrl = '/Report/B/BD0022.aspx';
    var T1LastRec = null;

    // 物料分類
    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    //合約識別碼
    var st_Mcontid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0022/GetMcontidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 200;
    //廠商代碼
    var AgennoComboGet = '/api/BD0022/GetAgennoCombo';
    var T1FormAgenno = Ext.create('WEBAPP.form.AgenNoCombo_1', {
        name: 'P4',
        id: 'P4',
        fieldLabel: '廠商代碼',
        allowBlank: true,
        limit: 20,
        queryUrl: AgennoComboGet,
        storeAutoLoad: true,
        insertEmptyRow: true,
        labelWidth: mLabelWidth,
        width: mWidth,
        //   padding: '0 1 0 1',
        listeners: {
            focus: function (field, event, eOpts) {
                T1Query.getForm().findField('P0').setValue('');
                if (!field.isExpanded) {
                    setTimeout(function () {
                        field.expand();
                    }, 300);
                }
            }
        }
    });

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
                    fieldLabel: '訂單號碼範圍',
                    name: 'P0',
                    enforceMaxLength: true,
                    maxLength: 15,
                    labelWidth: 100,
                    width: 200
                }, {
                    xtype: 'textfield',
                    fieldLabel: '至',
                    name: 'P0_1',
                    enforceMaxLength: true,
                    maxLength: 15,
                    labelWidth: 8,
                    width: 108
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
                    fieldLabel: '合約識別碼',
                    id: 'P2',
                    name: 'P2',
                    store: st_Mcontid,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    multiSelect: false,
                    labelWidth: 80,
                    width: 200
                }]
            }, {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'datefield',
                    fieldLabel: '訂單日期區間',
                    id: 'P3',
                    name: 'P3',
                    labelWidth: 100,
                    width: 200
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    id: 'P3_1',
                    name: 'P3_1',
                    labelWidth: 8,
                    width: 108
                },
                    T1FormAgenno,
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }]
            }]
        }]
    });

    function setComboMatClass() {
        Ext.Ajax.request({
            url: MatClassComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_matclass = data.etts;
                    if (tb_matclass.length > 0) {
                        for (var i = 0; i < tb_matclass.length; i++) {
                            st_matclass.add({ VALUE: tb_matclass[i].VALUE, COMBITEM: tb_matclass[i].COMBITEM });
                        }
                        if (tb_matclass.length == 1) {
                            T1Query.getForm().findField('P1').setValue(tb_matclass[0].VALUE);
                        }

                        if (menuLink == "BG0013") {
                            T1Query.getForm().findField('P1').setValue("02");
                        }
                    }
                }
            }
        });
    }
    function setDefaultPoTime() {
        // 訂單日期區間
        Ext.Ajax.request({
            url: '/api/BD0022/GetPoTime',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_poTime = data.etts;
                    if (tb_poTime.length > 0) {
                        T1Query.getForm().findField('P3').setValue(tb_poTime[0].VALUE);
                        T1Query.getForm().findField('P3_1').setValue(tb_poTime[0].TEXT);
                    }
                    else {
                    }
                }
            }
        });
    }
    setComboMatClass();
    setDefaultPoTime();

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'PO_TIME', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'PO_AMT', type: 'string' },
            { name: 'MEMO', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'PO_NO', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0022/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p0_1: T1Query.getForm().findField('P0_1').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').rawValue,
                    p3_1: T1Query.getForm().findField('P3_1').rawValue,
                    p4: T1Query.getForm().findField('P4').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯出是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('export').setDisabled(false);
                } else {
                    msglabel('查無符合條件的資料!');
                    Ext.getCmp('export').setDisabled(true);
                    Ext.getCmp('btnReport').setDisabled(true);
                }
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
            id: 'export',
            text: '匯出',
            hidden: false, //現有系統僅提供列印，故匯出功能隱藏
            disabled: true,
            handler: function () {
                var p = new Array();
                p.push({ name: 'FN', value: '檢查未到訂單' });
                p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                p.push({ name: 'p0_1', value: T1Query.getForm().findField('P0_1').getValue() });
                p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                p.push({ name: 'p3', value: T1Query.getForm().findField('P3').rawValue });
                p.push({ name: 'p3_1', value: T1Query.getForm().findField('P3_1').rawValue });
                p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });
                PostForm(T1GetExcel, p);
                msglabel('訊息區:匯出完成');
            }
        }, {
            text: '清單列印',
            id: 'btnListReport',
            name: 'btnDetailReport',
            //  disabled: true,
            handler: function () {
                showReport(2);
            }
        }, {
            text: '列印',
            id: 'btnReport',
            name: 'btnReport',
            disabled: true,
            handler: function () {
                showReport(1);
            }
        }, {
                text: '清單列印(選取)',
                id: 'btnListReport2',
                name: 'btnDetailReport2',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        showReport(3);
                    }
            }
        }]
    });
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
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
            }],
        selModel: {
            checkOnly: false,
            allowDeselect: true,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "訂單號碼",
            dataIndex: 'PO_NO',
            style: 'text-align:left',
            align: 'left',
            width: 130
        }, {
            text: "訂單日期",
            dataIndex: 'PO_TIME',
            style: 'text-align:left',
            align: 'left',
            width: 80
        }, {
            text: "廠商代碼",
            dataIndex: 'AGEN_NO',
            style: 'text-align:left',
            align: 'left',
            width: 80
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAMEC',
            style: 'text-align:left',
            align: 'left',
            width: 200
        }, {
            text: "訂購總價",
            dataIndex: 'PO_AMT',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            style: 'text-align:left',
            align: 'left',
            width: 500
        }],
        listeners: {
            selectionchange: function (model, records) {
                T2Store.removeAll();
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1LastRec != null) {
                    T1Form.loadRecord(records[0]);
                    T2Load();
                    Ext.getCmp('btnReport').setDisabled(false);
                }
                else
                    Ext.getCmp('btnReport').setDisabled(true);
                if (T1Grid.getSelection().length > 1) {
                    Ext.getCmp('btnListReport2').setDisabled(false);
                    Ext.getCmp('btnReport').setDisabled(true);
                }
            }
        }
    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME', type: 'string' },
            { name: 'M_PURUN', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'PO_QTY', type: 'string' },
            { name: 'PO_PRICE', type: 'string' },
            { name: 'PO_AMT', type: 'string' },
            { name: 'MEMO', type: 'string' },
            { name: 'DELI_STATUS', type: 'string' }
        ]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0022/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    po_no: T1LastRec.data.PO_NO,
                    chk_deli_y: T1Form.getForm().findField('CHK_DELI_Y').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T2Load() {
        T2Tool.moveFirst();
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true
    });
    // 查詢結果資料列表
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            style: 'text-align:left',
            align: 'left',
            width: 80,
            renderer: function (val, meta, record) {
                if (record.data['DELI_STATUS'] == 'Y') // 已進貨完則以藍字顯示
                    return '<font color=blue>' + val + '</font>';
                else
                    return val;
            }
        }, {
            text: "藥材名稱",
            dataIndex: 'MMNAME',
            style: 'text-align:left',
            align: 'left',
            width: 150,
            renderer: function (val, meta, record) {
                if (record.data['DELI_STATUS'] == 'Y') // 已進貨完則以藍字顯示
                    return '<font color=blue>' + val + '</font>';
                else
                    return val;
            }
        }, {
            text: "包裝",
            dataIndex: 'M_PURUN',
            style: 'text-align:left',
            align: 'left',
            width: 60
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            style: 'text-align:left',
            align: 'left',
            width: 60
        }, {
            text: "訂單數量",
            dataIndex: 'PO_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "訂單單價",
            dataIndex: 'PO_PRICE',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "小計",
            dataIndex: 'PO_AMT',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            style: 'text-align:left',
            align: 'left',
            width: 500
        }]
    });
    //重寫displayfield支援format屬性
    Ext.override(Ext.form.DisplayField, {
        getValue: function () {
            return this.value;
        },
        setValue: function (v) {
            this.value = v;
            this.setRawValue(this.formatValue(v));
            return this;
        },
        formatValue: function (v) {
            ;
            if (this.dateFormat && Ext.isDate(v)) {
                return v.dateFormat(this.dateFormat);
            }
            if (this.numberFormat && typeof v == 'string') {
                return Ext.util.Format.number(v, this.numberFormat);
            }
            return v;
        }
    });

    var mLabelWidth = 160;
    var mWidth = 360;
    var T1Form = Ext.create('Ext.form.Panel', {
        xtype: 'form',
        bodyStyle: 'padding:5px 5px 0',
        layout: {
            type: 'table',
            columns: 2,
            border: true,
            bodyBorder: true
        },
        bodyPadding: '5 5 0 0',
        autoScroll: true,
        frame: false,
        defaults: {
            labelAlign: 'right',
            readOnly: true,
            labelWidth: 80,
            width: 250,
            padding: '4 0 4 0',
            msgTarget: 'side'
        },
        defaultType: 'displayfield',// textfield
        items: [
            {
                fieldLabel: '合計',
                name: 'PO_AMT',
                numberFormat: '0,000'
            }, {
                xtype: 'checkbox',
                name: 'CHK_DELI_Y',
                width: 130,
                boxLabel: '顯示已進貨完項目',
                inputValue: 'Y',
                checked: false,
                padding: '0 4 0 8',
                readOnly: false,
                listeners:
                {
                    change: function (rg, nVal, oVal, eOpts) {
                        if (T1Grid.getSelection().length == 1) {
                            T2Load();
                        }
                    }
                }
            }
        ]
    });


    function showReport(mode) {
        var pono = "";
        if (mode == "3") {
            var selection = T1Grid.getSelection();
            $.map(selection, function (item, key) {
                pono += item.get('PO_NO') + ',';
            })
        }
        else {
            if (T1LastRec != null)
                pono = T1LastRec.data.PO_NO;
        }
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl 
                + '?rdlc=' + mode
                + '&PO_NO=' + pono
                + '&p0=' + T1Query.getForm().findField('P0').getValue()
                + '&p0_1=' + T1Query.getForm().findField('P0_1').getValue()
                + '&p1=' + T1Query.getForm().findField('P1').getValue()
                + '&p2=' + T1Query.getForm().findField('P2').getValue()
                + '&p3=' + T1Query.getForm().findField('P3').rawValue
                + '&p3_1=' + T1Query.getForm().findField('P3_1').rawValue
                + '&p4=' + T1Query.getForm().findField('P4').getValue()
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
            region: 'north',
            layout: 'fit',
            collapsible: false,
            title: '',
            height: '40%',
            border: false,
            items: [T1Grid]
        }, {
            itemId: 't1Form',
            region: 'north',
            collapsible: false,
            title: '',
            height: '6%',
            //  split: true,
            items: [T1Form]
        }, {
            itemId: 't2Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            height: '54%',
            split: true,
            items: [T2Grid]
        }]
    });

    Ext.getCmp('export').setDisabled(true);
    Ext.getCmp('btnReport').setDisabled(true);

    
});