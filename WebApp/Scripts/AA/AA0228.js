﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "戰備調整作業";

    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_getlogininfo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0228/GetLoginInfo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_Flowid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0228/GetFlowidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_Matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0228/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today;
    }
    function get6monthday() {
        var rtnDay = addMonths(new Date(), -6);
        return rtnDay;
    }
    function addMonths(date, months) {
        date.setMonth(date.getMonth() + months);
        return date;
    }
    var T1QueryForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: '調整單號',
                labelAlign: 'right',
                name: 'PP',
                id: 'PP'
            }, {
                xtype: 'datefield',
                fieldLabel: '調整日期區間',
                labelAlign: 'right',
                name: 'P0',
                id: 'P0',
                vtype: 'dateRange',
                dateRange: { end: 'P1' },
                padding: '0 4 0 4',
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
                padding: '0 4 0 4',
                value: getToday()
            }, {
                xtype: 'combo',
                fieldLabel: '狀態',
                labelAlign: 'right',
                name: 'P2',
                id: 'P2',
                store: st_Flowid,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '物料分類',
                labelAlign: 'right',
                name: 'P3',
                id: 'P3',
                store: st_Matclass,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE'
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                T1Load();
                msglabel('訊息區:');
            }
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                msglabel('訊息區:');
            }
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'DOCTYPE', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'APPID', type: 'string' },
            { name: 'APPDEPT', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'USEID', type: 'string' },
            { name: 'USEDEPT', type: 'string' },
            { name: 'FRWH', type: 'string' },
            { name: 'TOWH', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'APPLY_KIND', type: 'string' },
            { name: 'APPLY_NOTE', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'FLOWID_N', type: 'string' },
            { name: 'MAT_CLASS_N', type: 'string' },
            { name: 'FRWH_N', type: 'string' },
            { name: 'TOWH_N', type: 'string' },
            { name: 'APPLY_KIND_N', type: 'string' },
            { name: 'APP_NAME', type: 'string' },
            { name: 'APPDEPT_NAME', type: 'string' },
            { name: 'APPTIME_T', type: 'string' },
            { name: 'MR1', type: 'string' },
            { name: 'MR2', type: 'string' },
            { name: 'MR3', type: 'string' },
            { name: 'MR4', type: 'string' },
            { name: 'AGEN_NO_N', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0228/AllM',
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
                    p0: T1QueryForm.getForm().findField('P0').rawValue,
                    p1: T1QueryForm.getForm().findField('P1').rawValue,
                    p2: T1QueryForm.getForm().findField('P2').getValue(), //狀態
                    p3: T1QueryForm.getForm().findField('P3').getValue(), //物料分類
                    pp: T1QueryForm.getForm().findField('PP').getValue()
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
                itemId: 'add', text: '新增', handler: function () {
                    T1Set = '/api/AA0228/CreateM';
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                    TATabs.setActiveTab('Form');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AA0228/UpdateM';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }
            , {
                itemId: 'delete', text: '撤銷', disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })
                        Ext.MessageBox.confirm('撤銷', '是否確定撤銷調整單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0228/DeleteM',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            Ext.MessageBox.alert('訊息', '撤銷調整單號<br>' + name + '成功');
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
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
            },
            {
                itemId: 'apply', text: '執行過帳', disabled: true, handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })
                        Ext.MessageBox.confirm('執行過帳', '是否確定執行過帳？單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0228/Apply',
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
                                            msglabel('訊息區:執行過帳成功');
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
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCM');
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("DOCNO");
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField('DOCNO_D').setValue('系統自編');
            f.findField('FLOWID_N').setValue('未過帳');
            f.findField('APPTIME_T').setValue(st_getlogininfo.getAt(0).get('TWN_SYSDATE'));
            f.findField('APP_NAME').setValue(st_getlogininfo.getAt(0).get('USERNAME'));
            f.findField('FLOWID').setValue('1');
            f.findField('DOCTYPE').setValue('WT');
            f.findField('MAT_CLASS').setValue('01');
            f.findField('MAT_CLASS').setReadOnly(false);
        }
        else {
            u = f.findField('DOCNO');
        }
        f.findField('x').setValue(x);
        f.findField('APPLY_NOTE').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
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
            items: [T1QueryForm]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        selModel: {
            checkOnly: false,
            allowDeselect: true,
            injectCheckbox: 'first',
            mode: 'MULTI',
            listeners: {
                selectionchange: function (selectionModel, selected, options) {
                    if (selected.length > 0) {
                        T1Grid.down('#delete').setDisabled(false);
                        T1Grid.down('#apply').setDisabled(false);
                        for (var i = 0; i < selected.length; i++) {
                            if (selected[i].data.FLOWID != '1') {
                                T1Grid.down('#delete').setDisabled(true);
                                T1Grid.down('#apply').setDisabled(true);
                            }
                        }
                    }
                }
            }
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "調整單號",
            dataIndex: 'DOCNO',
            width: 140
        }, {
            text: "狀態",
            dataIndex: 'FLOWID_N',
            width: 120
        }, {
            text: "調整日期",
            dataIndex: 'APPTIME_T',
            width: 100
        }, {
            text: "備註",
            dataIndex: 'APPLY_NOTE',
            width: 180
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
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        //T1Grid.down('#delete').setDisabled(T1Rec === 0);
        //T1Grid.down('#apply').setDisabled(T1Rec === 0);
        T2Grid.down('#add').setDisabled(T1Rec === 0);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('DOCNO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            f.findField('MAT_CLASS').setReadOnly(true);
            T1F1 = f.findField('DOCNO').getValue();
            f.findField('DOCNO_D').setValue(T1F1);
            T1F2 = f.findField('MAT_CLASS').getValue();
            T1F3 = f.findField('FLOWID').getValue();

            if (T1F3 == '1') {
                T1Grid.down('#edit').setDisabled(false);
                T2Grid.down('#add').setDisabled(false);
            }
            else {
                T1Grid.down('#edit').setDisabled(true);
                T2Grid.down('#add').setDisabled(true);
            }

        }
        else {
            T1Form.getForm().reset();
            T1F1 = '';
            T1F2 = '';
        }
        T2Cleanup();
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
            name: 'DOCTYPE',
            xtype: 'hidden'
        }, {
            name: 'FLOWID',
            xtype: 'hidden'
        }, {
            name: 'FRWH',
            xtype: 'hidden'
        }, {
            name: 'APPLY_KIND',
            xtype: 'hidden'
        }, {
            name: 'APPID',
            xtype: 'hidden'
        }, {
            name: 'APPDEPT',
            xtype: 'hidden'
        }, {
            name: 'USEID',
            xtype: 'hidden'
        }, {
            name: 'USEDEPT',
            xtype: 'hidden'
        }, {
            name: 'DOCNO',
            xtype: 'hidden'
        }, {
            name: 'APPTIME',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '單據號碼',
            name: 'DOCNO_D'
        }, {
            xtype: 'displayfield',
            fieldLabel: '狀態',
            name: 'FLOWID_N'
        }, {
            xtype: 'datefield',
            fieldLabel: '調整日期',
            name: 'APPTIME_T',
            readOnly: true
        }, {
            xtype: 'combo',
            fieldLabel: '物料分類',
            name: 'MAT_CLASS',
            store: st_Matclass,
            queryMode: 'local',
            displayField: 'TEXT',
            valueField: 'VALUE',
            anyMatch: true,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            typeAhead: true,
            forceSelection: true,
            queryMode: 'local',
            triggerAction: 'all',
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '建立人員',
            name: 'APP_NAME'
        }, {
            xtype: 'textareafield',
            fieldLabel: '備註',
            name: 'APPLY_NOTE',
            enforceMaxLength: true,
            maxLength: 100,
            height: 70,
            readOnly: true
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)

                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );

                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    f2.findField('APPTIME').setValue(f2.findField('APPTIME_T').getValue());
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T1QueryForm.getForm().reset();
                            var v = action.result.etts[0];
                            T1QueryForm.getForm().findField('PP').setValue(v.DOCNO);
                            r.set(v);
                            T1Store.insert(0, r);
                            r.commit();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "A":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料核撥成功');
                            break;
                        case "D":
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            break;
                    }
                    T1Cleanup();
                    T1Load();
                    TATabs.setActiveTab('Query');
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
        TATabs.setActiveTab('Query');
    }

    //Detail
    var T2Rec = 0;
    var T2LastRec = null;

    var GetLOT_NO = '/api/AA0228/GetLOT_NO';
    var GetM_DISCPERC = '/api/AA0228/GetM_DISCPERC';
    var GetM_CONTPRICE = '/api/AA0228/GetM_CONTPRICE';

    var DISCPERC = '';
    var CONTPRICE = '';
    var NEW_APVQTY = '';

    var LOT_NO_Store = Ext.create('Ext.data.Store', {  //調帳庫別的store
        fields: ['LOT_NO', 'EXP_DATE', 'INV_QTY']
    });

    function SetLOT_NO(MMCODE, loadForm) { //建立批號的下拉式選單
        Ext.Ajax.request({
            url: GetLOT_NO,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: MMCODE
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var lot_nos = data.etts;
                    LOT_NO_Store.removeAll();
                    if (lot_nos.length > 0) {
                        // LOT_NO_Store.add({ LOT_NO: '', EXP_DATE: '', INV_QTY: '' });
                        for (var i = 0; i < lot_nos.length; i++) {
                            LOT_NO_Store.add({ LOT_NO: lot_nos[i].LOT_NO, EXP_DATE: lot_nos[i].EXP_DATE, INV_QTY: lot_nos[i].INV_QTY });
                        }
                        // loadForm為true時才預代第一筆資料
                        if (loadForm) {
                            T2Form.getForm().findField('LOT_NO').forceSelection = false;
                            T2Form.getForm().findField("LOT_NO").setValue(lot_nos[0].LOT_NO);
                            T2Form.getForm().findField("EXP_DATE").setValue(lot_nos[0].EXP_DATE);
                            T2Form.getForm().findField("EXP_DATE_T").setValue(lot_nos[0].EXP_DATE);
                            var qty_temp = lot_nos[0].INV_QTY;
                            T2Form.getForm().findField("INV_QTY").setValue(qty_temp);
                            
                            if (T2Form.getForm().findField("APVQTY").getValue() == null) {
                                T2Form.getForm().findField("APVQTY").setValue(qty_temp);
                            }
                        }

                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    
    function SetM_DISCPERC() { //取得進貨單價
        Ext.Ajax.request({
            url: GetM_DISCPERC,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: T2Form.getForm().findField("MMCODE").getValue()
            },
            success: function (response) {
                DISCPERC = response.responseText.replace(new RegExp('"', 'g'), '');
                if (DISCPERC == null) {
                    DISCPERC = 0;
                }
                NEW_APVQTY = T2Form.getForm().findField("APVQTY").getValue();
                if (T2Form.getForm().findField('IN_PRICE').getValue() != '' &&
                    T2Form.getForm().findField('IN_PRICE').getValue() != null) {
                    T2Form.getForm().findField("C_AMT").setValue(Math.round(NEW_APVQTY * DISCPERC * 100) / 100);
                }
                else if (T2Form.getForm().findField('CONTPRICE').getValue() != '' &&
                    T2Form.getForm().findField('CONTPRICE').getValue() != null) {
                    T2Form.getForm().findField("C_AMT").setValue(Math.round(NEW_APVQTY * CONTPRICE * 100) / 100);
                }
            },
            failure: function (response, options) {
            }
        });
    }

    function SetM_CONTPRICE() { //取得合約單價
        Ext.Ajax.request({
            url: GetM_CONTPRICE,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: T2Form.getForm().findField("MMCODE").getValue()
            },
            success: function (response) {
                CONTPRICE = response.responseText.replace(new RegExp('"', 'g'), '');
                //Ext.getCmp("CONTPRICE_1").setBoxLabel('合約單價: ' + CONTPRICE);
            },
            failure: function (response, options) {
            }
        });
    }

    var T2QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0228/GetMMCodeDocd', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
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
            labelWidth: 100
        },
        border: false,
        items: [
            T2QueryMMCode,
            {
                xtype: 'button',
                text: '查詢',
                handler: T2Load
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

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'LOT_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'C_TYPE', type: 'string' },
            { name: 'C_STATUS', type: 'string' },
            { name: 'C_RNO', type: 'string' },
            { name: 'C_AMT', type: 'string' },
            { name: 'C_UP', type: 'string' },
            { name: 'ITEM_NOTE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'BALANCE', type: 'string' },
            { name: 'LOT_NO_N', type: 'string' },
            { name: 'EXP_DATE_T', type: 'string' }
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
            url: '/api/AA0228/AllD',
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
                    p0: T1F1,
                    p1: T2Query.getForm().findField('P1').getValue()
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
        //width: 220,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0228/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'INV_QTY', 'M_CONTPRICE', 'M_DISCPERC', 'AGEN_NAMEC'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('MAT_CLASS').getValue(),
                p2: T1Form.getForm().findField('DOCNO').getValue()
            };
        },
        listeners: {
            change: function (combo, newValue, oldValue, eOpts) {
                var f = T2Form.getForm();
                f.findField("MMNAME_C").setValue("");
                f.findField("MMNAME_E").setValue("");
                f.findField("AGEN_NAMEC").setValue("");
                f.findField("BASE_UNIT").setValue("");
                f.findField('IN_PRICE').setValue("");
                f.findField('CONTPRICE').setValue("");
                DISCPERC = '';
                CONTPRICE = '';

                if (newValue != '') {
                    Ext.Ajax.request({
                        url: '/api/AA0228/GetSelectMmcodeDetail',
                        method: reqVal_g,
                        params: {
                            MMCODE: newValue,
                            MAT_CLASS: T1Form.getForm().findField('MAT_CLASS').getValue(),
                            DOCNO: T1Form.getForm().findField('DOCNO').getValue()
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                var tb_data = data.etts;
                                if (tb_data.length > 0) {
                                    f.findField('MMNAME_C').setValue(tb_data[0].MMNAME_C);
                                    f.findField('MMNAME_E').setValue(tb_data[0].MMNAME_E);
                                    f.findField("AGEN_NAMEC").setValue(tb_data[0].AGEN_NAMEC);
                                    f.findField('BASE_UNIT').setValue(tb_data[0].BASE_UNIT);
                                    f.findField('INV_QTY').setValue(tb_data[0].INV_QTY);
                                    f.findField('M_CONTPRICE').setValue(tb_data[0].M_CONTPRICE);
                                    f.findField('IN_PRICE').setValue(tb_data[0].M_DISCPERC);
                                    f.findField('CONTPRICE').setValue(tb_data[0].M_CONTPRICE);

                                    DISCPERC = tb_data[0].M_DISCPERC;
                                    CONTPRICE = tb_data[0].M_CONTPRICE;

                                    SetLOT_NO(tb_data[0].MMCODE, true);
                                }
                            }
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });
                }
                else {
                    f.findField("LOT_NO").setValue("");
                    f.findField("LOT_NO").clearInvalid();
                    f.findField("EXP_DATE").setValue("");
                    f.findField("EXP_DATE_T").setValue("");
                    f.findField("APVQTY").setValue("");
                    f.findField("INV_QTY").setValue("");
                }
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
            name: 'SEQ',
            xtype: 'hidden'
        }, {
            name: 'EXP_DATE',
            xtype: 'hidden'
        }, {
            name: 'C_UP',
            xtype: 'hidden'
        }, {
            name: 'M_CONTPRICE',
            xtype: 'hidden'
        }, {
            name: 'INOUT',
            xtype: 'hidden'
        }, {
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
                            popMmcodeForm_14(viewport, '/api/AA0228/GetMmcode', {
                                MMCODE: f.findField("MMCODE").getValue(),
                                MAT_CLASS: T1Form.getForm().findField('MAT_CLASS').getValue(),
                                DOCNO: T1Form.getForm().findField('DOCNO').getValue()
                            }, setMmcode);
                        }
                    }
                }
            ]
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
            fieldLabel: '廠商',
            name: 'AGEN_NAMEC'
        }, {
            xtype: 'container',
            layout: 'hbox',
            padding: '0 7 7 0',
            items: [
                {
                    xtype: 'hidden',
                    name: 'LOT_NO_N'
                },
                {
                    xtype: 'combo',
                    fieldLabel: '批號',
                    name: 'LOT_NO',
                    store: LOT_NO_Store,
                    displayField: 'LOT_NO',
                    valueField: 'LOT_NO',
                    //width: 220,
                    allowBlank: false,
                    enforceMaxLength: true,
                    maxLength: 20,
                    queryMode: 'local',
                    fieldCls: 'required',
                    readOnly: true,
                    forceSelection: false,
                    listeners: {
                        select: function (c, r, i, e) {
                            var f = T2Form.getForm();
                            f.findField("LOT_NO").setValue(r.get('LOT_NO'));
                            f.findField("EXP_DATE").setValue(r.get('EXP_DATE'));
                            f.findField("EXP_DATE_T").setValue(r.get('EXP_DATE'));
                            f.findField("INV_QTY").setValue(r.get('INV_QTY'));
                            f.findField("APVQTY").setValue(f.findField("INV_QTY").getValue());
                            if (f.findField("APVQTY").getValue() == null) {
                                f.findField("APVQTY").setValue(r.get('INV_QTY'));
                            }
                        }
                    }
                }
            ]
        }, {
            xtype: 'datefield',
            fieldLabel: '效期',
            name: 'EXP_DATE_T',
            fieldCls: 'required',
            allowBlank: false, // 欄位為必填
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '效期數量',
            name: 'INV_QTY'
        }, {
            fieldLabel: '調整數量',
            name: 'APVQTY',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 10,
            maskRe: /[0-9]/,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                change: function (field, newValue, oldValue) {
                    NEW_APVQTY = newValue;
                }
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '單位',
            name: 'BASE_UNIT'
        }, {
            xtype: 'hidden',
            //fieldLabel: '進貨單價', //白底顯示
            name: 'IN_PRICE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '合約單價', //白底顯示
            name: 'CONTPRICE',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '調整總金額',
            name: 'C_AMT'
        }, {
            xtype: 'textareafield',
            fieldLabel: '備註',
            name: 'ITEM_NOTE',
            enforceMaxLength: true,
            maxLength: 50,
            height: 70,
            readOnly: true
        }],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                if ((T2Form.getForm().findField("MMCODE").getValue()) == "" ||
                    (T2Form.getForm().findField("MMCODE").getValue() == null)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                }
                else if ((T2Form.getForm().findField("LOT_NO").rawValue) == "" ||
                    (T2Form.getForm().findField("LOT_NO").rawValue == null)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>批號不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>批號不可為空</span>，請重新輸入。");
                } else if ((T2Form.getForm().findField("APVQTY").getValue()) == "" ||
                    (T2Form.getForm().findField("APVQTY").getValue() == null)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>調整數量不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>調整數量不可為空</span>，請重新輸入。");
                }
                else {
                    if ((this.up('form').getForm().findField('LOT_NO').rawValue == '' || this.up('form').getForm().findField('LOT_NO').rawValue == null)
                        || (this.up('form').getForm().findField('EXP_DATE_T').getValue() == '' || this.up('form').getForm().findField('EXP_DATE_T').getValue() == null)) {
                        Ext.Msg.alert('提醒', '批號及效期為必填');
                    }
                    else {
                        if (this.up('form').getForm().findField('APVQTY').getValue() == '0')
                            Ext.Msg.alert('提醒', '調整數量不可為0');
                        else {
                            if (this.up('form').getForm().isValid()) {
                                var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                                Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                    if (btn === 'yes') {
                                        T2Submit();
                                    }
                                }
                                );
                            }
                            else
                                Ext.Msg.alert('提醒', '輸入格式不正確');
                        }
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
            f.findField('EXP_DATE').setValue(f.findField('EXP_DATE_T').rawValue);
            f.submit({
                url: T11Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    f2.findField('EXP_DATE').setValue(f2.findField('EXP_DATE_T').getValue());
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T2Store.insert(0, r);
                            r.commit();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T2Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            break;
                    }
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
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield" || fc.xtype == "datefield") {
                fc.setReadOnly(true);
            }
        });
        f.findField('MMCODE').setReadOnly(true);
        f.findField('APVQTY').setReadOnly(true);
        f.findField('ITEM_NOTE').setReadOnly(true);
        f.findField('LOT_NO').forceSelection = false;
        T2Form.down('#T2Cancel').hide();
        T2Form.down('#T2Submit').hide();
        viewport.down('#form').setTitle('瀏覽');
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
                    T11Set = '/api/AA0228/CreateD';
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T11Set = '/api/AA0228/UpdateD';
                    setFormT2("U", '修改');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let name = '';
                        let docno = '';
                        let seq = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('SEQ') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //    seq += item.get('SEQ') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('SEQ') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                            seq += item.get('SEQ') + ',';
                        })
                        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0228/DeleteD',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            //Ext.MessageBox.alert('訊息', '刪除項次<br>' + name + '成功');
                                            msglabel('資料刪除成功');
                                            //T2Store.removeAll();
                                            T2Grid.getSelectionModel().deselectAll();
                                            T2Load();
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
                                        }
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
            }
        ]
    });
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T2Name);
        viewport.down('#form').expand();
        
        TATabs.setActiveTab('Form');
        var f2 = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            f2.reset();
            var r = Ext.create('WEBAPP.model.ME_DOCD');
            T2Form.loadRecord(r);
            f2.findField('DOCNO').setValue(T1F1);
            u = f2.findField("MMCODE");

            f2.findField('LOT_NO').forceSelection = true;
            f2.findField('EXP_DATE_T').setReadOnly(true);
            f2.findField('LOT_NO').setValue('');
            f2.findField('EXP_DATE_T').setValue('');
            LOT_NO_Store.removeAll();

            f2.findField("APVQTY").setValue('0');
            f2.findField("INV_QTY").setValue('0');
            f2.findField('MMNAME_C').setValue('');
            f2.findField('MMNAME_E').setValue('');
            f2.findField('AGEN_NAMEC').setValue('');
            f2.findField('MMCODE').setReadOnly(false);
        }
        else {
            SetLOT_NO(f2.findField("MMCODE").getValue(), false);
            u = f2.findField('APVQTY');
            //SetM_CONTPRICE();
            //SetM_DISCPERC();
            f2.findField("APVQTY").setValue(f2.findField("INV_QTY").getValue());

            f2.findField('LOT_NO').forceSelection = true;
            f2.findField('EXP_DATE_T').setReadOnly(true);
        }

        f2.findField('x').setValue(x);
        f2.findField('MMCODE').setReadOnly(false);
        f2.findField('LOT_NO').setReadOnly(false);
        f2.findField('LOT_NO_N').setReadOnly(false);
        f2.findField('APVQTY').setReadOnly(false);
        // f2.findField('EXP_DATE_T').setReadOnly(false);
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
            layout: 'fit',
            items: [T2Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "換入/換出",
            dataIndex: 'INOUT_N',
            width: 80,
            sortable: true
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80,
            sortable: true
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 150,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 180,
            sortable: true
        }, {
            text: "廠商",
            dataIndex: 'AGEN_NAMEC',
            width: 170
        }, {
            text: "批號",
            dataIndex: 'LOT_NO',
            width: 80
        }, {
            text: "效期",
            dataIndex: 'EXP_DATE_T',
            width: 80
        }, {
            text: "調整數量",
            dataIndex: 'APVQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50
        }, {
            text: "單價",
            dataIndex: 'C_UP',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "調整總金額",
            dataIndex: 'C_AMT',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "備註",
            dataIndex: 'ITEM_NOTE',
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
                        TATabs.setActiveTab('Form');
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
        T2Grid.down('#delete').setDisabled(T2Rec === 0);
        if (T2LastRec) {
            isNew = false;
            var f2 = T2Form.getForm();
            f2.findField('LOT_NO').forceSelection = false;
            T2Form.loadRecord(T2LastRec);
            f2.findField('x').setValue('U');
            if (T1F3 == '1') {
                T2Grid.down('#edit').setDisabled(false);
                T2Grid.down('#delete').setDisabled(false);
            }
            else {
                T2Grid.down('#edit').setDisabled(true);
                T2Grid.down('#delete').setDisabled(true);
            }
        }
        else {
            T2Form.getForm().reset();
        }
    }

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
            items: [T1QueryForm]
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
            width: 310,
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [TATabs]
        }
        ]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    T1QueryForm.getForm().findField('P0').focus();
    //viewport.down('#form').collapse();
});
