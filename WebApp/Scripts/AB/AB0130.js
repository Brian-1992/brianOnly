Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "補藥通知單管理";
    var reportUrl = '/Report/A/AB0130.aspx';
    var T1Rec;
    var T1LastRec = null;
    var T1Name = "";

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'APPDEPT', type: 'string' },
            { name: 'APPDEPT_N', type: 'string' },
            { name: 'APP_INID_N', type: 'string' }
        ]
    });

    var inid_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0130/GetInidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, autoLoad: true
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: false,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [
            {
                xtype: 'textfield',
                fieldLabel: '補藥單編號',
                id: 'P0',
                name: 'P0',
            }, {
                xtype: 'datefield',
                fieldLabel: '補藥日期',
                id: 'D1',
                name: 'D1',
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                id: 'D2',
                name: 'D2',
            }, {
                xtype: 'checkboxgroup',
                rowspan: 3,
                columns: 1,
                id: 'P3',
                items: [
                    { boxLabel: '待送出', name: 'P3', inputValue: '1', width: 100, checked: true },
                    { boxLabel: '待核可', name: 'P3', inputValue: '2', width: 100, checked: false },
                    { boxLabel: '已核可', name: 'P3', inputValue: '3', width: 100, checked: false },
                ],
                getValue: function () {
                    var val = [];
                    this.items.each(function (item) {
                        if (item.checked)
                            val.push(item.inputValue);
                    });
                    return val.toString();
                }
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: T1Load
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在DOCNO
            }
        }]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 30, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'ASC' }, { property: 'APPTIME', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0130/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: Ext.util.Format.date(T1Query.getForm().findField('D1').getValue(), 'Ymd'),
                    p2: Ext.util.Format.date(T1Query.getForm().findField('D2').getValue(), 'Ymd'),
                    p3: T1Query.getForm().findField('P3').getValue()
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
        Ext.getCmp('btnT2edit').setDisabled(true);
        Ext.getCmp('btnT2del').setDisabled(true);
        T1Tool.moveFirst();
        viewport.down('#form').collapse();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
            itemId: 'add', text: '新增補藥單', disabled: false, handler: function () {
                Ext.MessageBox.confirm('新增確認', '是否新增補藥單？', function (btn, text) {
                    if (btn === 'yes') {
                        Ext.Ajax.request({
                            url: '/api/AB0130/InsertM',
                            method: reqVal_p,
                            //async: true,
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    T2Store.removeAll();
                                    T1Grid.getSelectionModel().deselectAll();
                                    T1Load();
                                    msglabel('訊息區:新增補藥單成功');
                                }
                                else
                                    Ext.MessageBox.alert('錯誤', data.msg);
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            }
                        });
                    }
                });
            }
        }, {
            itemId: 'edit', text: '修改補藥單', disabled: true, handler: function () {
                msglabel('訊息區:');
                setFormT1("U", '修改');
            }
        }, {
            itemId: 'cancel', text: '取消補藥單', disabled: true, handler: function () {
                var selection = T1Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                }
                else {
                    let name = '';
                    let docno = '';
                    $.map(selection, function (item, key) {
                        name += '「' + item.get('DOCNO') + '」<br>';
                        docno += item.get('DOCNO') + ',';
                    })
                    Ext.MessageBox.confirm('取消補藥單', '補藥單：' + name + '，是否確定取消？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AB0130/Cancel',
                                method: reqVal_p,
                                params: {
                                    DOCNO: docno
                                },
                                //async: true,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        T2Store.removeAll();
                                        T1Grid.getSelectionModel().deselectAll();
                                        T1Load();
                                        msglabel('訊息區:取消補藥單成功');
                                    }
                                    else
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    });
                }
            }
        }, {
            itemId: 'send', text: '送出補藥單', disabled: true, handler: function () {
                var selection = T1Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                }
                else {
                    let name = '';
                    let docno = '';
                    let mmcode = '';
                    $.map(selection, function (item, key) {
                        name += '「' + item.get('DOCNO') + '」<br>';
                        docno += item.get('DOCNO') + ',';
                        mmcode += item.get('DOCNO') + ',';
                    })
                    Ext.MessageBox.confirm('送出補藥單', '補藥單：' + name + '，是否確定送出？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AB0130/Apply',
                                method: reqVal_p,
                                params: {
                                    DOCNO: docno
                                },
                                //async: true,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        T2Store.removeAll();
                                        T1Grid.getSelectionModel().deselectAll();
                                        T1Load();
                                        msglabel('訊息區:補藥單送出完成');
                                    }
                                    else
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    });
                }
            }
        }, {
            itemId: 'print', text: '補藥單列印', disabled: true, handler: function () {
                Ext.Ajax.request({
                    url: '/api/AB0130/ChkDetailCnt',
                    method: reqVal_p,
                    params: {
                        DOCNO: T1LastRec.data["DOCNO"]
                    },
                    //async: true,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            if (data.msg == 'Y')
                                showReport(T1LastRec.data["DOCNO"]);
                            else
                                Ext.MessageBox.alert('訊息', '補藥單尚無明細資料,請確認!');
                        }
                        else
                            Ext.MessageBox.alert('錯誤', data.msg);
                    },
                    failure: function (response) {
                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                    }
                });
            }
        }]
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
            items: [T1Tool]
        }],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "補藥單編號",
            dataIndex: 'DOCNO',
            width: 130
        }, {
            text: "補藥日期",
            dataIndex: 'APPTIME',
            width: 150,
            style: 'text-align:left',
            align: 'center'
        }, {
            text: "補藥單位",
            dataIndex: 'APPDEPT_N',
            width: 130
        }, {
            text: "申請人責任中心",
            dataIndex: 'APP_INID_N',
            width: 130
        }, {
            text: "狀態",
            dataIndex: 'STATUS',
            width: 100
        }, {
            text: "補藥核可日期",
            dataIndex: 'APVTIME',
            width: 150,
            style: 'text-align:left',
            align: 'center'
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T2Form.setVisible(false);
                        TATabs.setActiveTab('Form');
                    }
                }
            },
            selectionchange: function (model, records) {
                viewport.down('#form').expand();
                T1Rec = records;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });

    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        var f = T1Form.getForm();
        u = f.findField('DOCNO');
        f.findField('x').setValue(x);
        //f.findField('APPDEPT').setValue(st_getlogininfo.getAt(0).get('INID'));
        inid_store.load({ callback: function () { f.findField('APPDEPT').setValue(f.findField('APPDEPT').getValue()); } });
        f.findField('APPDEPT').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec.length === 0);
        T1Grid.down('#cancel').setDisabled(T1Rec.length === 0);
        T1Grid.down('#send').setDisabled(T1Rec.length === 0);
        T1Grid.down('#print').setDisabled(T1Rec.length === 0);
        T2Grid.down('#btnT2add').setDisabled(T1Rec.length === 0);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        if (T1LastRec) {
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            T1F1 = T1LastRec.data["DOCNO"];
            inid_store.load();
            f.findField('APPDEPT').setReadOnly(true);

            // 勾選項目所有資料之 IS_DEL = 'N' 才可打開 edit、cancel、send、btnT2add
            // 只勾選一筆且該資料之 IS_DEL = 'S' 才可打開 print
            var isDelCheck_N = 0;
            var isDelCheck_S = 0;
            for (var i = 0; i < T1Rec.length; i++) {
                if (T1Rec[i].data["IS_DEL"] !== 'N') {
                    isDelCheck_N++;
                }
                if (T1Rec[i].data["IS_DEL"] !== 'S') {
                    isDelCheck_S++;
                }
            }

            if (isDelCheck_N == 0) {
                T1Grid.down('#edit').setDisabled(false);
                T1Grid.down('#cancel').setDisabled(false);
                T1Grid.down('#send').setDisabled(false);
                T2Grid.down('#btnT2add').setDisabled(false);
            }
            else {
                T1Grid.down('#edit').setDisabled(true);
                T1Grid.down('#cancel').setDisabled(true);
                T1Grid.down('#send').setDisabled(true);
                T2Grid.down('#btnT2add').setDisabled(true);
            }

            if (isDelCheck_S == 0 && T1Grid.getSelection().length == 1) {
                T1Grid.down('#print').setDisabled(false);
            }
            else {
                T1Grid.down('#print').setDisabled(true);
            }
        }
        else {
            T1Form.getForm().reset();
            T1F1 = '';
        }

        T2Cleanup();
        T2Load(true);
    }

    var T1Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            name: 'x',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '補藥單編號',
            name: 'DOCNO'
        }, {
            xtype: 'displayfield',
            fieldLabel: '補藥日期',
            name: 'APPTIME'
        }, {
            xtype: 'combo',
            fieldLabel: '補藥單位',
            name: 'APPDEPT',
            store: inid_store,
            queryMode: 'local',
            displayField: 'TEXT',
            valueField: 'VALUE',
            //  tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            //  anyMatch: true,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            typeAhead: true,
            forceSelection: true,
            queryMode: 'local',
            triggerAction: 'all',
            fieldCls: 'required'
        }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AB0130/UpdateM',
                                method: reqVal_p,
                                params: {
                                    docno: T1Form.getForm().findField('DOCNO').getValue(),
                                    appdept: T1Form.getForm().findField('APPDEPT').getValue()
                                },
                                //async: true,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        T2Store.removeAll();
                                        T1Grid.getSelectionModel().deselectAll();
                                        T1Load();
                                        msglabel('訊息區:修改補藥單成功');
                                    }
                                    else
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    });
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
        T2Cleanup();
    }

    //Detail
    var T2Rec = 0;
    var T2LastRec = null;
    var MaxValue = 0;
    var InvQty = 0;

    var T2QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0130/GetMMCodeDocd', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('DOCNO').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 50
        },
        border: false,
        items: [
            T2QueryMMCode,
            {
                xtype: 'checkbox',
                name: 'P2',
                width: 130,
                boxLabel: '點滴',
                inputValue: '1',
                checked: false,
                padding: '0 4 0 8'
            }, {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    T2Load(true);
                }
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P1').focus();
                }
            }
        ]
    });

    function setMmcode(args) {
        var f = T2Form.getForm();
        if (args.MMCODE !== '') {
            f.findField("MMCODE").setValue(args.MMCODE);
            T2FormMMCode.doQuery();
            var func = function () {
                var record = T2FormMMCode.store.getAt(0);
                T2FormMMCode.select(record);
                T2FormMMCode.fireEvent('select', this, record);
                T2FormMMCode.store.un('load', func);
            };
            T2FormMMCode.store.on('load', func);
        }
    }

    var T2FormMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '藥材代碼',
        readOnly: true,
        allowBlank: false,
        fieldCls: 'required',
        width: 230,
        matchFieldWidth: false,
        listConfig: { width: 180 },
        margin: '0 0 10 0',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0130/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'M_NHIKEY', 'SUPPLY_WHNO', 'TO_INV_QTY', 'BASE_UNIT_DESC', 'AGEN_NAMEC', 'FR_INV_QTY', 'BASE_UNIT', 'CASENO', 'E_ITEMARMYNO', 'HIGH_QTY', 'UPRICE', 'E_CODATE_T', 'ORDERKIND', 'CASENO'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                appdept: T1Form.getForm().findField('APPDEPT').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                T2Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                T2Form.getForm().findField('M_NHIKEY').setValue(r.get('M_NHIKEY'));
                T2Form.getForm().findField('SUPPLY_WHNO').setValue(r.get('SUPPLY_WHNO'));
                T2Form.getForm().findField('TO_INV_QTY').setValue(r.get('TO_INV_QTY'));
                T2Form.getForm().findField('BASE_UNIT_DESC').setValue(r.get('BASE_UNIT_DESC'));
                T2Form.getForm().findField('AGEN_NAMEC').setValue(r.get('AGEN_NAMEC'));
                T2Form.getForm().findField('FR_INV_QTY').setValue(r.get('FR_INV_QTY'));
                T2Form.getForm().findField('BASE_UNIT').setValue(r.get('BASE_UNIT'));
                T2Form.getForm().findField('CASENO').setValue(r.get('CASENO'));
                T2Form.getForm().findField('E_ITEMARMYNO').setValue(r.get('E_ITEMARMYNO'));
                T2Form.getForm().findField('HIGH_QTY').setValue(r.get('HIGH_QTY'));
                T2Form.getForm().findField('UPRICE').setValue(r.get('UPRICE'));
                T2Form.getForm().findField('E_CODATE').setValue(r.get('E_CODATE_T'));
                T2Form.getForm().findField('ORDERKIND').setValue(r.get('ORDERKIND'));
                T2Form.getForm().findField('CASENO').setValue(r.get('CASENO'));

                T2Form.getForm().findField('APPQTY').setValue('0');
                T2Form.getForm().findField('TOTAL_AMT').setValue('0');
            }
        }
    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'MMCODE', type: 'string' },      //藥材代碼
            { name: 'MMNAME_C', type: 'string' },      //藥材名稱
            { name: 'M_NHIKEY', type: 'string' },      //健保代碼
            { name: 'SUPPLY_WHNO', type: 'string' },      //撥發單位
            { name: 'TO_INV_QTY', type: 'string' },      //使用單位現存量
            { name: 'BASE_UNIT_DESC', type: 'string' },      //出貨單位
            { name: 'APPTIME', type: 'string' },      //補藥日期
            { name: 'AGEN_NAMEC', type: 'string' },      //供應商
            { name: 'FR_INV_QTY', type: 'string' },      //撥發單位庫存量
            { name: 'APPQTY', type: 'string' },      //補藥數量
            { name: 'BASE_UNIT', type: 'string' },      //單位
            { name: 'CASENO', type: 'string' },      //合約
            { name: 'E_ITEMARMYNO', type: 'string' },      //聯標項次
            { name: 'HIGH_QTY', type: 'string' },      //最高補藥量
            { name: 'UPRICE', type: 'string' },      //單價
            { name: 'E_CODATE', type: 'string' },      //合約到期日
            { name: 'ORDERKIND', type: 'string' },      //採購類別
            { name: 'TOTAL_AMT', type: 'string' },      //小計
            { name: 'WH_NAME', type: 'string' },      //倉儲單位
            { name: 'SEQ', type: 'string' }      //明細項次
        ]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'SEQ', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0130/AllD',
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
                    p0: T1F1,
                    p1: T2Query.getForm().findField('P1').getValue(),
                    p2: T2Query.getForm().findField('P2').rawValue,
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T2Load(loadFirst) {
        try {
            if (loadFirst) {
                T2Tool.moveFirst();
            } else {
                T2Store.load({
                    params: {
                        start: 0
                    }
                });
            }

        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
        //viewport.down('#form').collapse();
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
            text: '新增',
            name: 'btnT2add', id: 'btnT2add',
            disabled: true,
            handler: function () {
                T1Set = '/api/AB0130/InsertD';
                setFormT2("I", '新增');
                TATabs.setActiveTab('Form');
            }
        }, {
            text: '修改',
            name: 'btnT2edit', id: 'btnT2edit',
            disabled: true,
            handler: function () {
                T1Set = '/api/AB0130/UpdateD';
                msglabel('訊息區:');
                setFormT2("U", '修改');
            }
        }, {
            text: '刪除',
            name: 'btnT2del', id: 'btnT2del',
            disabled: true,
            handler: function () {
                Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                    if (btn === 'yes') {
                        Ext.Ajax.request({
                            url: '/api/AB0130/DeleteD',
                            method: reqVal_p,
                            params: {
                                DOCNO: T2LastRec.data.DOCNO,
                                SEQ: T2LastRec.data.SEQ
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    msglabel('訊息區:刪除成功');
                                    T2Load(true);
                                } else {
                                    Ext.MessageBox.alert('錯誤', data.msg);
                                }
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            }
                        });
                    }
                });
            }
        }]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T2Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "藥材代碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
        }, {
            text: "藥材名稱",
            dataIndex: 'MMNAME_C',
            width: 200
        }, {
            text: "補藥量",
            dataIndex: 'APPQTY',
            width: 100,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 120
        }, {
            text: "單價",
            dataIndex: 'UPRICE',
            width: 120,
            sortable: true
        }, {
            text: "小計",
            dataIndex: 'TOTAL_AMT',
            width: 120
        }, {
            text: "使用單位現存量",
            dataIndex: 'TO_INV_QTY',
            width: 120
        }, {
            text: "倉儲單位",
            dataIndex: 'WH_NAME',
            width: 80
        }, {
            text: "明細項次",
            dataIndex: 'SEQ',
            width: 80
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false);
                        T2Form.setVisible(true);
                        TATabs.setActiveTab('Form');
                    }
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });

    //點選T1Grid一筆資料的動作
    function setFormT2a() {
        T2Grid.down('#btnT2edit').setDisabled(T2Rec === 0);
        T2Grid.down('#btnT2del').setDisabled(T2Rec === 0);
        if (T2LastRec) {
            viewport.down('#form').expand();
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('MMCODE');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');

            if (T1LastRec.data["IS_DEL"] == 'N') {
                Ext.getCmp('btnT2edit').setDisabled(false);
                Ext.getCmp('btnT2del').setDisabled(false);
            }
            else {
                Ext.getCmp('btnT2edit').setDisabled(true);
                Ext.getCmp('btnT2del').setDisabled(true);
            }
        }
        else {
            T2Form.getForm().reset();
            //viewport.down('#form').collapse();
        }
    }


    //按下新增(I)/修改(U) btn
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        var f2 = T2Form.getForm();
        if (x == 'I') {
            isNew = true;
            f2.reset();
            var r = Ext.create('T2Model');
            //var r = Ext.create('WEBAPP.model.ME_DOCD');
            T2Form.loadRecord(r);
            f2.findField('DOCNO').setValue(T1F1);
            u = f2.findField("MMCODE");
            f2.findField('MMCODE').setReadOnly(false);
        } else {
            u = f2.findField('APPQTY');

        }
        f2.findField('x').setValue(x);
        f2.findField('TO_INV_QTY').setReadOnly(false);
        f2.findField('APPQTY').setReadOnly(false);
        // f2.findField('DOCNO').setValue(T1F1);

        T2Form.down('#T2Cancel').setVisible(true);
        T2Form.down('#T2Submit').setVisible(true);
        u.focus();
    }

    var T2Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T2b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 110,
            width: 230,
        },
        defaultType: 'textfield',
        items: [{
            xtype: 'container',
            layout: {
                type: 'table',
                columns: 2
            },
            items: [{
                name: 'x',
                xtype: 'hidden'
            }, {
                name: 'DOCNO',
                submitValue: true,
                xtype: 'hidden'
            }, {
                name: 'SEQ',
                submitValue: true,
                xtype: 'hidden'
            },
                T2FormMMCode
                ,
            {
                xtype: 'button',
                itemId: 'btnMmcode',
                iconCls: 'TRASearch',
                //hidden: true,
                handler: function () {
                    var f = T2Form.getForm();
                    if (!f.findField("MMCODE").readOnly) {
                        popMmcodeForm_14(viewport, '/api/AB0130/GetMmcode', {
                            MMCODE: f.findField("MMCODE").getValue(),
                            //MAT_CLASS: T1Form.getForm().findField('MAT_CLASS').getValue(),
                            MAT_CLASS: '01',
                            //WH_NO: T1Form.getForm().findField('TOWH').getValue(),
                            //ISCONTID3: T1Form.getForm().findField('ISCONTID3').getValue()
                            APP_INID: T1Form.getForm().findField('APPDEPT').getValue()
                        }, setMmcode);
                    }
                }
            }
             , {
                xtype: 'displayfield',
                fieldLabel: '藥材名稱',
                name: 'MMNAME_C',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '健保代碼',
                name: 'M_NHIKEY',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '撥發單位',
                name: 'SUPPLY_WHNO',
                readOnly: true
            }, {
                xtype: 'numberfield',
                fieldLabel: '使用單位現存量',
                name: 'TO_INV_QTY',
                fieldCls: 'required',
                readOnly: true,
                submitValue: true,
                minValue: 0,
                allowBlank: false,
                decimalPrecision: 0
            }, {
                xtype: 'displayfield',
                fieldLabel: '出貨單位',
                name: 'BASE_UNIT_DESC',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '供應商',
                name: 'AGEN_NAMEC',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '撥發單位庫存量',
                name: 'FR_INV_QTY',
                readOnly: true
            }, {
                xtype: 'numberfield',
                fieldLabel: '補藥數量',
                name: 'APPQTY',
                fieldCls: 'required',
                readOnly: true,
                submitValue: true,
                minValue: 1,
                allowBlank: false,
                decimalPrecision: 0,
                listeners: {
                    blur: function (filed, event, eOpts) {
                        var appqty = T2Form.getForm().findField('APPQTY').getValue();
                        var uprice = T2Form.getForm().findField('UPRICE').getValue();
                        var qtyAmt = Math.round(appqty * uprice);
                        T2Form.getForm().findField('TOTAL_AMT').setValue(qtyAmt);
                    }
                }
            }, {
                xtype: 'displayfield',
                fieldLabel: '單位',
                name: 'BASE_UNIT',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '合約',
                name: 'CASENO',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '聯標項次',
                name: 'E_ITEMARMYNO',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '最高補藥量',
                name: 'HIGH_QTY',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '單價',
                name: 'UPRICE',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '合約到期日',
                name: 'E_CODATE',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '採購類別',
                name: 'ORDERKIND',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '合約案號',
                name: 'CASENO',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '小計',
                name: 'TOTAL_AMT',
                readOnly: true
            }]
        }],
        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true,
            handler: function () {
                if (T2Form.getForm().isValid()) { // 檢查T2Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    if (this.up('form').getForm().findField('APPQTY').getValue() == '0') {
                        Ext.Msg.alert('提醒', '補藥數量不可為0');
                    } else {
                        Ext.MessageBox.confirm('提示', '是否確定儲存?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                                isNew = false;
                            }
                        });
                    }
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'T2Cancel', text: '取消', hidden: true, handler: function () {
                T2Cleanup();
                viewport.down('#form').collapse();
            }
        }]
    });

    function T2Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T2Form.getForm();
        f.reset();
        f.findField('APPQTY').setReadOnly(true);
        f.getFields().each(function (fc) {
            fc.setReadOnly(true);
        });
        T2Form.down('#T2Cancel').hide();
        T2Form.down('#T2Submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT2a();
    }

    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    var data = Ext.decode(action.response.responseText);
                    if (data.success) {
                        if (data.msg.toString() == "") {
                            switch (f2.findField("x").getValue()) {
                                case "I":
                                    T2Load(true);
                                    msglabel('訊息區:資料新增成功');
                                    isNew = false;
                                    break;
                                case "U":
                                    T2Load(true);
                                    msglabel('訊息區:資料修改成功');
                                    isNew = false;
                                    break;
                            }
                            T2Cleanup();
                        } else {
                            Ext.MessageBox.alert('警示', data.msg);
                        }
                    }
                    else
                        Ext.MessageBox.alert('錯誤', data.msg);

                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }

    function showReport(P0) {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                + '?P0=' + P0
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

    //view 
    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
        },
        items: [{
            itemId: 'Query',
            title: '查詢',
            items: [T1Query]
        }, {
            itemId: 'Form',
            title: '瀏覽',
            items: [T1Form, T2Form]
        }]
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
            items: [{
                //  xtype:'container',
                region: 'center',
                layout: {
                    type: 'border',
                    padding: 0
                },
                collapsible: false,
                title: '',
                split: true,
                width: '80%',
                flex: 1,
                minWidth: 50,
                minHeight: 140,
                items: [
                    {
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '30%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '70%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 480,
            title: '瀏覽',
            border: false,
            collapsed: true,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [TATabs]
            // items: [T2Form]
        }]
    });

    function right(str, num) {
        return str.substring(str.length - num, str.length)
    }
    T1Load(); // 進入畫面時自動載入一次資料

});
