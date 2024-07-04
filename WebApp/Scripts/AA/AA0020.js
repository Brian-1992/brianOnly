Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/AA0020/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "近效期更換";

    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_Agen = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0020/GetAgenCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    var st_Flowid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0020/GetFlowidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_Closeflag = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0020/GetCloseflagCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_Procid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0020/GetProcidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_Yyymm = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0020/GetYyymmCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    // 查詢欄位
    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        id: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0020/GetMmCodeCombo',
        labelWidth: 50,
        width: 200
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 70
        },
        border: false,
        items: [
            {
                xtype: 'combo',
                fieldLabel: '狀態',
                labelAlign: 'right',
                name: 'P0',
                id: 'P0',
                store: st_Flowid,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                labelWidth: 50,
                width: 200,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>'
            },
            T1QueryMMCode,
            {
                xtype: 'combo',
                fieldLabel: '廠商',
                labelAlign: 'right',
                name: 'P2',
                id: 'P2',
                store: st_Agen,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                labelWidth: 50,
                width: 200,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>'
            }, {
                xtype: 'combo',
                fieldLabel: '效期',
                labelAlign: 'right',
                name: 'P3',
                id: 'P3',
                store: st_Yyymm,
                queryMode: 'local',
                displayField: 'VALUE',
                valueField: 'VALUE',
                labelWidth: 50,
                width: 130
            },
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
                    f.findField('P1').focus();
                }
            }]
    });
    //var T1QueryForm = Ext.widget({
    //    xtype: 'form',
    //    layout: 'form',
    //    frame: false,
    //    title: '',
    //    autoScroll: true,
    //    bodyPadding: '5 5 0',
    //    fieldDefaults: {
    //        labelAlign: 'right',
    //        msgTarget: 'side',
    //        labelWidth: 90
    //    },
    //    defaultType: 'textfield',
    //    items: [
    //        {
    //            xtype: 'combo',
    //            fieldLabel: '狀態',
    //            name: 'P0',
    //            id: 'P0',
    //            store: st_Flowid,
    //            queryMode: 'local',
    //            displayField: 'COMBITEM',
    //            valueField: 'VALUE'
    //        },
    //        T1QueryMMCode,
    //        {
    //            xtype: 'combo',
    //            fieldLabel: '廠商',
    //            name: 'P2',
    //            id: 'P2',
    //            store: st_Agen,
    //            queryMode: 'local',
    //            displayField: 'COMBITEM',
    //            valueField: 'VALUE',
    //            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>'
    //        },
    //        {
    //            xtype: 'combo',
    //            fieldLabel: '效期',
    //            name: 'P3',
    //            id: 'P3',
    //            store: st_Yyymm,
    //            queryMode: 'local',
    //            displayField: 'VALUE',
    //            valueField: 'VALUE'
    //        }
    //    ],
    //    buttons: [{
    //        itemId: 'query', text: '查詢',
    //        handler: function () {
    //            T1Load();
    //            msglabel('訊息區:');
    //        }
    //    }, {
    //        itemId: 'clean', text: '清除', handler: function () {
    //            var f = this.up('form').getForm();
    //            f.reset();
    //            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
    //            msglabel('訊息區:');
    //        }
    //    }]
    //});
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'WARNYM', type: 'string' },
            { name: 'LOT_NO', type: 'string' },
            { name: 'MEMO', type: 'string' },
            { name: 'CLOSEFLAG', type: 'string' },
            { name: 'CREATE_DATE', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_DATE', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'EXP_QTY', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'M_AGENNO', type: 'string' },
            { name: 'MMCODE2', type: 'string' },
            { name: 'MMNAME_C2', type: 'string' },
            { name: 'MMNAME_E2', type: 'string' },
            { name: 'BASE_UNIT2', type: 'string' },
            { name: 'EXP_QTY2', type: 'string' },
            { name: 'EXP_DATE2', type: 'string' },
            { name: 'EXP_DATE_N', type: 'string' },
            { name: 'EXP_DATE_T', type: 'string' },
            { name: 'CLOSEFLAG_N', type: 'string' },
            { name: 'FLOWID_N', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'DOCNO_M', type: 'string' },
            { name: 'DOCNO_E', type: 'string' },
            { name: 'RDOCNO', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'EXP_DATE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0020/AllM',
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
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
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
        viewport.down('#form').collapse();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'apply', text: '執行過帳', disabled: true, handler: function () {
                    Ext.MessageBox.confirm('執行過帳', '是否確定執行過帳？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AA0020/Apply',
                                method: reqVal_p,
                                params: {
                                    DOCNO: T1F14,
                                    MMCODE: T1F1,
                                    EXP_DATE: T1F2T,
                                    LOT_NO: T1F3 
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        //Ext.MessageBox.alert('訊息', '過帳成功');
                                        T2Store.removeAll();
                                        T1Load();
                                        msglabel('訊息區:過帳成功');
                                    }
                                    else
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    }
                    );

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
        //selModel: {
        //    checkOnly: false,
        //    injectCheckbox: 'first',
        //    mode: 'MULTI'
        //},
        //selType: 'checkboxmodel',
        columns: [{
                xtype: 'rownumberer'
            }, {
                text: "狀態",
                dataIndex: 'FLOWID_N',
                width: 120
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100,
                sortable: true
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 150,
                sortable: true
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200,
                sortable: true
            }, {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                width: 50,
                sortable: true
            }, {
                text: "數量",
                dataIndex: 'EXP_QTY',
                style: 'text-align:left',
                width: 80, align: 'right'
            }, {
                text: "效期",
                dataIndex: 'EXP_DATE_T',
                width: 80
            }, {
                text: "廠商",
                dataIndex: 'M_AGENNO',
                width: 180
            },

            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T2Form.setVisible(false);
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#apply').setDisabled(T1Rec === 0);
        T2Grid.down('#add').setDisabled(T1Rec === 0);
        viewport.down('#form').expand();
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('MMCODE');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            T1F1 = f.findField('MMCODE').getValue();
            T1F2 = f.findField('EXP_DATE_N').getValue();
            T1F2T = f.findField('EXP_DATE_T').getValue();
            T1F2O = f.findField('EXP_DATE').getValue();
            T1F3 = f.findField('LOT_NO').getValue();
            T1F4 = f.findField('AGEN_NO').getValue();
            T1F5 = f.findField('CLOSEFLAG').getValue();
            T1F6 = f.findField('MMCODE2').getValue();
            T1F7 = f.findField('DOCNO_M').getValue();
            T1F8 = f.findField('DOCNO_E').getValue();
            T1F9 = f.findField('MMCODE').getValue();
            T1F10 = f.findField('EXP_QTY').getValue();
            T1F11 = f.findField('MMNAME_C').getValue();
            T1F12 = f.findField('MMNAME_E').getValue();
            T1F13 = f.findField('BASE_UNIT').getValue();
            T1F14 = f.findField('RDOCNO').getValue();
            T1F15 = f.findField('MEMO').getValue();
            T1F16 = f.findField('M_CONTPRICE').getValue();
            if (T1F5 == 'Y') {
                T1Grid.down('#apply').setDisabled(true);
            }
            else {
                T1Grid.down('#apply').setDisabled(false);
            }
            if (T1F7 == '') {
                T2Grid.down('#add').setDisabled(false);
            }
            else {
                T2Grid.down('#add').setDisabled(true);
            }
        }
        else {
            T1Form.getForm().reset();
            T1F1 = '';
            T1F2 = '';
            T1F3 = '';
            T1F4 = '';
            T1F5 = '';
            T1F6 = '';
            T1F7 = '';
            T1F8 = '';
            T1F9 = '';
            T1F10 = '';
        }
        T2Load();

    }

    // 顯示明細/新增/修改輸入欄
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
                name: 'AGEN_NO',
                xtype: 'hidden'
            }, {
                name: 'EXP_DATE_N',
                xtype: 'hidden'
            }, {
                name: 'EXP_DATE',
                xtype: 'hidden'
            }, {
                name: 'CLOSEFLAG',
                xtype: 'hidden'
            }, {
                name: 'MMCODE2',
                xtype: 'hidden'
            }, {
                name: 'DOCNO_M',
                xtype: 'hidden'
            }, {
                name: 'DOCNO_E',
                xtype: 'hidden'
            }, {
                name: 'RDOCNO',
                xtype: 'hidden'
            }, {
                xtype: 'displayfield',
                fieldLabel: '廠商',
                name: 'M_AGENNO'
            }, {
                xtype: 'displayfield',
                fieldLabel: '院內碼',
                name: 'MMCODE'
            }, {
                xtype: 'displayfield',
                fieldLabel: '中文品名',
                name: 'MMNAME_C'
            }, {
                xtype: 'displayfield',
                fieldLabel: '英文品名',
                name: 'MMNAME_E'
            }, {
                xtype: 'displayfield',
                fieldLabel: '數量',
                name: 'EXP_QTY'
            }, {
                xtype: 'displayfield',
                fieldLabel: '單位',
                name: 'BASE_UNIT'
            }, {
                xtype: 'displayfield',
                fieldLabel: '效期',
                name: 'EXP_DATE_T'
            }, {
                xtype: 'displayfield',
                fieldLabel: '藥品批號',
                name: 'LOT_NO'
            }, {
                xtype: 'displayfield',
                fieldLabel: '合約單價',
                name: 'M_CONTPRICE',
                submitValue: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '備註',
                name: 'MEMO'
            }
            //, {
            //    xtype: 'displayfield',
            //    fieldLabel: '建立人員',
            //    name: 'CREATE_USER'
            //}, {
            //    xtype: 'displayfield',
            //    fieldLabel: '建立日期',
            //    name: 'CREATE_TIME'
            //}
        ]
    });
    
    //Detail
    var T2Rec = 0;
    var T2LastRec = null;

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'APVTIME', type: 'string' },
            { name: 'APVID', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_DATE', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'LOT_NO', type: 'string' },
            { name: 'ITEM_NOTE', type: 'string' },
            { name: 'C_TYPE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'float' },
            { name: 'M_AGENNO', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'AGEN_NAMEE', type: 'string' },
            { name: 'EXP_DATET', type: 'string' },
            { name: 'DOCNO_M', type: 'string' },
            { name: 'DOCNO_E', type: 'string' },
            { name: 'M_DISCPERC', type: 'float' },
            { name: 'MMCODE_O', type: 'string' },
            { name: 'LOT_NO_O', type: 'string' },
            { name: 'EXP_DATE_O', type: 'string' },
            { name: 'EXP_QTY_O', type: 'string' }
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'SEQ', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0020/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1F14
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T2Load() {
        try {
            T2Store.load({
                params: {
                    start: 0
                }
            });
        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
        //viewport.down('#form').collapse();
    }
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
        fieldLabel: '院內碼',
        readOnly: true,
        allowBlank: false,
        fieldCls: 'required',
        width: 220,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0020/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'M_CONTPRICE', 'M_DISCPERC'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('AGEN_NO').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                T2Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                T2Form.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
                T2Form.getForm().findField('BASE_UNIT').setValue(r.get('BASE_UNIT'));
                T2Form.getForm().findField('M_CONTPRICE').setValue(r.get('M_CONTPRICE'));
                T2Form.getForm().findField('CONTPRICE').setValue(r.get('M_CONTPRICE'));
                T2Form.getForm().findField('M_DISCPERC').setValue(r.get('M_DISCPERC'));
            }
        }
    });
    var T2Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'vbox',
        frame: false,
        autoScroll: true,
        cls: 'T2b',
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
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            }, {
                name: 'DOCNO',
                xtype: 'hidden'
            }, {
                name: 'DOCNO_M',
                xtype: 'hidden'
            }, {
                name: 'DOCNO_E',
                xtype: 'hidden'
            }, {
                name: 'SEQ',
                xtype: 'hidden'
            }, {
                name: 'FLOWID',
                xtype: 'hidden'
            }, {
                name: 'MMCODE1',
                xtype: 'hidden'
            }, {
                name: 'EXP_QTY',
                xtype: 'hidden'
            }, {
                name: 'EXP_DATE',
                xtype: 'hidden'
            }, {
                name: 'M_DISCPERC',
                xtype: 'hidden'
            }, {
                name: 'MMCODE_O',
                xtype: 'hidden'
            }, {
                name: 'LOT_NO_O',
                xtype: 'hidden'
            }, {
                name: 'EXP_DATE_O',
                xtype: 'hidden'
            }, {
                name: 'EXP_QTY_O',
                xtype: 'hidden'
            }, {
                name: 'MEMO_O',
                xtype: 'hidden'
            }, {
                name: 'M_CONTPRICE',
                xtype: 'hidden'
            }, {
                xtype: 'combo',
                fieldLabel: '換藥方式',
                name: 'C_TYPE',
                store: st_Procid,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                listeners: {
                    select: function (c, r, eo) {
                        var f2 = T2Form.getForm();
                        if (r.get('VALUE') == '1') {
                            f2.findField('APVQTY').setReadOnly(false);
                            f2.findField('MMCODE').setReadOnly(false);
                            f2.findField('EXP_DATET').setReadOnly(false);
                            f2.findField('LOT_NO').setReadOnly(false);
                        }
                        else {
                            f2.findField('APVQTY').setReadOnly(true);
                            f2.findField('MMCODE').setReadOnly(true);
                            f2.findField('EXP_DATET').setReadOnly(true);
                            f2.findField('LOT_NO').setReadOnly(true);
                        }
                    }
                }
            },
            {
                xtype: 'container',
                layout: 'hbox',
                //padding: '0 7 7 0',
                items: [
                    T2FormMMCode,
                    {
                        xtype: 'button',
                        itemId: 'btnMmcode',
                        iconCls: 'TRASearch',
                        handler: function () {
                            var f = T2Form.getForm();
                            if (!f.findField("MMCODE").readOnly) {
                                popMmcodeForm(viewport, '/api/AA0020/GetMmcode', {
                                    MMCODE: f.findField("MMCODE").getValue()
                                }, setMmcode);
                            }
                        }
                    },

                ]
            }
            , {
                xtype: 'displayfield',
                fieldLabel: '中文品名',
                name: 'MMNAME_C'
            }, {
                xtype: 'displayfield',
                fieldLabel: '英文品名',
                name: 'MMNAME_E'
            }, {
                fieldLabel: '數量',
                name: 'APVQTY',
                allowBlank: false,
                fieldCls: 'required',
                enforceMaxLength: true,
                maxLength: 10,
                maskRe: /[0-9]/,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '單位',
                name: 'BASE_UNIT'
            }, {
                xtype: 'datefield',
                allowBlank: false,
                fieldCls: 'required',
                fieldLabel: '效期',
                name: 'EXP_DATET',
                readOnly: true
            }, {
                fieldLabel: '藥品批號',
                name: 'LOT_NO',
                fieldCls: 'required',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '合約單價',
                name: 'CONTPRICE'
            }, {
                xtype: 'textareafield',
                fieldLabel: '備註',
                name: 'ITEM_NOTE',
                enforceMaxLength: true,
                maxLength: 60,
                height: 120,
                readOnly: true
            }
        ],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                if (this.up('form').getForm().findField('C_TYPE').getValue() == '') {
                    Ext.Msg.alert('提醒', '換藥方式不可空白');
                }
                else {
                    if (this.up('form').getForm().findField('APVQTY').getValue() == '0' || this.up('form').getForm().findField('APVQTY').getValue() == '')
                        Ext.Msg.alert('提醒', '數量不可為0或空白');
                    else {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                            }
                        }
                        );
                    }
                }
            }
        }, {
            itemId: 'T2Cancel', text: '取消', hidden: true, handler: T2Cleanup
        }]
    });
    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T2Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();

                    f2.findField('EXP_DATE').setValue(f2.findField('EXP_DATET').getValue());
                    //switch (f2.findField("x").getValue()) {
                    //    case "U":
                    //        var v = action.result.etts[0];
                    //        r.set(v);
                    //        r.commit();
                    //        break;
                    //}
                    T1Load();
                    T2Cleanup();
                    T2Load();
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
                    }
                }
            });
        }
    }
    function T2Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T2Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield" || fc.xtype == "datefield" ) {
                fc.setReadOnly(true);
            }
        });
        f.findField('MMCODE').setReadOnly(true);
        T2Form.down('#T2Cancel').hide();
        T2Form.down('#T2Submit').hide();
        T2Form.down('#btnMmcode').setVisible(false);
        setFormT2a();
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'add', text: '新增', disabled: true, handler: function () {
                    T2Set = '../../../api/AA0020/CreateD';
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T2Set = '../../../api/AA0020/UpdateD';
                    setFormT2("U", '修改');
                }
            }
        ]
    });
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T2Name);
        var f2 = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            f2.reset();
            //var r = Ext.create('T2Model');
            var r = Ext.create('WEBAPP.model.ME_DOCEXP');
            T2Form.loadRecord(r);
            f2.findField('MMCODE').setValue(T1F1);
            f2.findField('EXP_DATE').setValue(T1F2T);
            f2.findField('EXP_DATET').setValue(T1F2T);
            f2.findField('LOT_NO').setValue(T1F3);
            f2.findField('DOCNO_M').setValue(T1F7);
            f2.findField('DOCNO_E').setValue(T1F8);
            f2.findField('MMCODE1').setValue(T1F9);
            f2.findField('EXP_QTY').setValue(T1F10);
            f2.findField('APVQTY').setValue(T1F10);
            f2.findField('MMNAME_C').setValue(T1F11);
            f2.findField('MMNAME_E').setValue(T1F12);
            f2.findField('BASE_UNIT').setValue(T1F13);
            f2.findField('MMCODE_O').setValue(T1F1);
            f2.findField('EXP_DATE_O').setValue(T1F2);
            f2.findField('LOT_NO_O').setValue(T1F3);
            f2.findField('MEMO_O').setValue(T1F15);
            f2.findField('EXP_QTY_O').setValue(T1F10);
            f2.findField('M_CONTPRICE').setValue(T1F16);
            f2.findField('CONTPRICE').setValue(T1F16);
            u = f2.findField("C_TYPE");
            f2.findField('C_TYPE').setReadOnly(false);
            f2.findField('APVQTY').setReadOnly(false);
            T2Form.down('#btnMmcode').setVisible(true);
        }
        else {
            u = f2.findField('ITEM_NOTE');
            f2.findField('C_TYPE').setReadOnly(false);
            if (f2.findField('C_TYPE').getValue() == "1") {
                f2.findField('APVQTY').setReadOnly(false);
                f2.findField('MMCODE').setReadOnly(false);
                f2.findField('EXP_DATET').setReadOnly(false);
                f2.findField('LOT_NO').setReadOnly(false);
            }
            else {
                f2.findField('APVQTY').setReadOnly(true);
                f2.findField('MMCODE').setReadOnly(true);
                f2.findField('EXP_DATET').setReadOnly(true);
                f2.findField('LOT_NO').setReadOnly(true);
            }
            f2.findField('APVQTY').setReadOnly(false);
        }

        f2.findField('x').setValue(x);
        f2.findField('ITEM_NOTE').setReadOnly(false);
        T2Form.down('#T2Cancel').setVisible(true);
        T2Form.down('#T2Submit').setVisible(true);
        u.focus();
    }
    

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T2Store,
        plain: true,
        loadMask: true,
        //autoScroll: true,
        cls: 'T2',
        //defaults: {
        //    layout: 'fit'
        //},
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }
        ],
        //selModel: {
        //    checkOnly: false,
        //    injectCheckbox: 'first',
        //    mode: 'MULTI'
        //},
        //selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 120,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150,
            sortable: true
        }, {
            text: "數量",
            dataIndex: 'APVQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50,
            sortable: true
        }, {
            text: "效期",
            dataIndex: 'EXP_DATET',
            width: 100
        }, {
            header: "",
            flex: 1
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false);
                        T2Form.setVisible(true);
                    }
                }
            },

            selectionchange: function (model, records) {
                viewport.down('#form').expand();
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });
    function setFormT2a() {
        T2Grid.down('#edit').setDisabled(T2Rec === 0);
        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
            f.findField('x').setValue('U');
            f.findField('CONTPRICE').setValue(T2LastRec.get('M_CONTPRICE'));
            //var u = f.findField('ID');
            //u.setReadOnly(true);
            //u.setFieldStyle('border: 0px');
            if (T1F5 === 'Y') {
                T2Grid.down('#add').setDisabled(true);
                T2Grid.down('#edit').setDisabled(true);
            }
            else {
                T2Grid.down('#add').setDisabled(false);
                T2Grid.down('#edit').setDisabled(false);
            }
        }
        else {
            T2Form.getForm().reset();
        }
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
            items: [{
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
                        height: '50%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
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
            width: 300,
            title: '瀏覽',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form, T2Form]
        }
        ]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
    viewport.down('#form').collapse();
});
