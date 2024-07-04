
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/AB0003/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "揀貨資料查詢作業";

    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";
    var T1F1 = "";

    var DOCNO = "";
    var v_DOCNO = "";
    var reportUrl1 = '/Report/C/CD00101.aspx';
    var reportUrl2 = '/Report/C/CD00102.aspx';
    var reportUrl3 = '/Report/C/CD00103.aspx';
    // 申請部門
    var appdept_store = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var appdept_Get = '../../../api/CD0010/GetAppdeptCombo';
    function setAppdeptComboData() {
        Ext.Ajax.request({
            url: appdept_Get,
            method: reqVal_p,
            params: {
                WH_NO: T1QueryForm.getForm().findField('T1QF0').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var etts = data.etts;
                    appdept_store.removeAll();
                    if (etts.length > 0) {
                        appdept_store.add({ VALUE: '', TEXT: '' });
                        var wh_no = null;
                        for (var i = 0; i < etts.length; i++) {
                            //if (wh_no == null) {
                            //    wh_no = etts[i].VALUE;
                            //}
                            appdept_store.add({ VALUE: etts[i].VALUE, TEXT: etts[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    // 揀貨人員
    var act_pick_userid_store = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var act_pick_userid_Get = '../../../api/CD0010/GetActPickUseridCombo';
    function setActPickUserIdComboData() {
        Ext.Ajax.request({
            url: act_pick_userid_Get,
            method: reqVal_p,
            params: {
                WH_NO: T1QueryForm.getForm().findField('T1QF0').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var etts = data.etts;
                    act_pick_userid_store.removeAll();
                    if (etts.length > 0) {
                        act_pick_userid_store.add({ VALUE: '', TEXT: '' });
                        var wh_no = null;
                        for (var i = 0; i < etts.length; i++) {
                            //if (wh_no == null) {
                            //    wh_no = etts[i].VALUE;
                            //}
                            act_pick_userid_store.add({ VALUE: etts[i].VALUE, TEXT: etts[i].TEXT });
                        }
                        //var combo = T1Query.getForm().findField('P0');
                        //combo.select(combo.getStore().getAt(0));    // 若有資料combo選取第一筆資料

                        //setMmcodeCombo(wh_no);
                    }
                    //else {
                    //    Ext.MessageBox.alert('錯誤', '查不到揀貨人員資料');
                    //    msglabel('訊息區:查不到揀貨人員資料');
                    //}
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 共用函式
    function getAddDate(addDays) {
        var dt = new Date();
        dt.setDate(dt.getDate() + addDays);
        return dt;
    }

    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    // 庫房號碼
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var WhnoComboGet = '../../../api/CD0006/GetWhnoCombo';
    function setComboData() {
        Ext.Ajax.request({
            url: WhnoComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        // whnoQueryStore.add({ VALUE: '', TEXT: '' });
                        var wh_no = null;
                        for (var i = 0; i < wh_nos.length; i++) {
                            if (wh_no == null) {
                                wh_no = wh_nos[i].VALUE;
                            }
                            whnoQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                        var combo = T1QueryForm.getForm().findField('T1QF0');
                        combo.select(combo.getStore().getAt(0));    // 若有資料combo選取第一筆資料
                        setAppdeptComboData(); // 載入申請部門資料
                        setActPickUserIdComboData(); // 載入揀貨人員資料
                        // setMmcodeCombo(wh_no); // 載入院內碼
                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '查不到你所屬庫房資料');
                        msglabel('訊息區:查不到你所屬庫房資料');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();



    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_docno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0003/GetDocnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    var st_pkdocno = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1F2
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0003/GetDocnopkCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        }
    });
    var st_pknote = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1F2
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0003/GetDocpknoteCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        }
    });
    var st_Flowid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0003/GetFlowidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_apply_kind = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0003/GetApplyKindCombo',
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
            url: '/api/AB0003/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_reason = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0003/GetReasonCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_towhcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0003/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_getlogininfo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0003/GetLoginInfo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    // 查詢欄位
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
                xtype: 'combo',
                store: whnoQueryStore,
                name: 'T1QF0',
                id: 'T1QF0',
                fieldLabel: '庫房號碼',
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                anyMatch: true,
                allowBlank: false, // 欄位為必填
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                fieldCls: 'required',
                multiSelect: false,
                blankText: "請選擇庫房號碼",
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                padding: '0 4 0 4',
                listeners: {
                    select: function (oldValue, newValue, eOpts) {

                        var wh_no = newValue.data.VALUE;
                        // setMmcodeCombo(wh_no); // 載入院內碼

                        setAppdeptComboData();
                        setActPickUserIdComboData(); // 戴入揀貨人員資料

                        var f = T1Query.getForm();
                        var u = f.findField('P4');
                        u.clearValue();
                        u.clearInvalid();
                    }
                }
            }, {
                xtype: 'datefield',
                id: 'T1QF1',
                name: 'T1QF1',
                fieldLabel: '揀貨日期',
                //value: getAddDate(-5),
                value: getToday(),
                //vtype: 'dateRange',
                //dateRange: { end: 'T1QF2' },
                fieldCls: 'required',
                padding: '0 4 0 4',
                listeners: {
                    change: function (field, nVal, oVal, eOpts) {
                        // T1Query.getForm().findField('P1').setMinValue(nVal);
                        // Check
                        // setValue('xxxx')
                    },
                    focus: function (field, event, eOpts) {
                        if (!field.isExpanded) // 改為按整個field即展開下拉選項,以方便在手機畫面點picker
                        {
                            setTimeout(function () {
                                field.expand(); // 若透過點picker下拉選項,會重複做expand,expand只需做一次就好   
                            }, 300);
                        }
                    }
                }
            }, {
                xtype: 'datefield',
                id: 'T1QF2',
                name: 'T1QF2',
                fieldLabel: '至',
                value: getToday(),
                //vtype: 'dateRange',
                //dateRange: { begin: 'T1QF1' },
                fieldCls: 'required',
                padding: '0 4 0 4',
                listeners: {
                    change: function (field, nVal, oVal, eOpts) {
                    },
                    focus: function (field, event, eOpts) {
                        if (!field.isExpanded) {
                            setTimeout(function () {
                                field.expand();
                            }, 300);
                        }
                    }
                }
            }, {
                xtype: 'datefield',
                id: 'T1QF3',
                name: 'T1QF3',
                fieldLabel: '出庫日期',
                //value: new Date(),
                //vtype: 'dateRange',
                //dateRange: { end: 'T1QF4' },
                // fieldCls: 'required',
                padding: '0 4 0 4',
                listeners: {
                    change: function (field, nVal, oVal, eOpts) {
                    },
                    focus: function (field, event, eOpts) {
                        if (!field.isExpanded) // 改為按整個field即展開下拉選項,以方便在手機畫面點picker
                        {
                            setTimeout(function () {
                                field.expand(); // 若透過點picker下拉選項,會重複做expand,expand只需做一次就好   
                            }, 300);
                        }
                    }
                }
            }, {
                xtype: 'datefield',
                id: 'T1QF4',
                name: 'T1QF4',
                fieldLabel: '至',
                //vtype: 'dateRange',
                //dateRange: { begin: 'T1QF3' },
                padding: '0 4 0 4',
                listeners: {
                    change: function (field, nVal, oVal, eOpts) {
                    },
                    focus: function (field, event, eOpts) {
                        if (!field.isExpanded) {
                            setTimeout(function () {
                                field.expand();
                            }, 300);
                        }
                    }
                }
            }, {
                xtype: 'textfield',
                fieldLabel: '申請單號',
                name: 'T1QF5',
                id: 'T1QF5',
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 100, // 可輸入最大長度為100
                padding: '0 4 0 4'
            }, {
                xtype: 'combo',
                store: appdept_store,
                name: 'T1QF12',
                id: 'T1QF12',
                fieldLabel: '申請部門',
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                anyMatch: true,
                //allowBlank: false, // 欄位為必填
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                multiSelect: false,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                padding: '0 4 0 4',
                listeners: {
                    select: function (oldValue, newValue, eOpts) {

                    }
                }
            }, {
                xtype: 'textfield',
                fieldLabel: '院內碼',
                name: 'T1QF6',
                id: 'T1QF6',
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 100, // 可輸入最大長度為100
                padding: '0 4 0 4'
            }, {
                xtype: 'combo',
                store: act_pick_userid_store,
                name: 'T1QF11',
                id: 'T1QF11',
                fieldLabel: '分配揀貨人員',
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                anyMatch: true,
                //allowBlank: false, // 欄位為必填
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                multiSelect: false,
                blankText: "請選擇揀貨人員",
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                padding: '0 4 0 4',
                listeners: {
                    select: function (oldValue, newValue, eOpts) {
                        //var wh_no = newValue.data.VALUE;
                        //setMmcodeCombo(wh_no);

                        //var f = T1Query.getForm();
                        //var u = f.findField('P4');
                        //u.clearValue();
                        //u.clearInvalid();
                    }
                }
            }, {
                xtype: 'combo',
                store: act_pick_userid_store,
                name: 'T1QF7',
                id: 'T1QF7',
                fieldLabel: '實際揀貨人員',
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                anyMatch: true,
                //allowBlank: false, // 欄位為必填
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                multiSelect: false,
                blankText: "請選擇揀貨人員",
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                padding: '0 4 0 4',
                listeners: {
                    select: function (oldValue, newValue, eOpts) {
                        //var wh_no = newValue.data.VALUE;
                        //setMmcodeCombo(wh_no);

                        //var f = T1Query.getForm();
                        //var u = f.findField('P4');
                        //u.clearValue();
                        //u.clearInvalid();
                    }
                }
            }, {
                xtype: 'fieldcontainer',
                defaultType: 'checkboxfield',
                layout: 'hbox', // hbox水平排列, hbox垂直排列
                fieldLabel: '揀貨差異',
                items: [
                    {
                        boxLabel: '無差異',
                        name: 'T1QF8_1',
                        id: 'T1QF8_01',
                        inputValue: 'Y',
                        padding: '0 4 0 4',
                        checked: true
                    }, {
                        boxLabel: '有差異',
                        name: 'T1QF8_2',
                        id: 'T1QF8_02',
                        inputValue: 'N',
                        padding: '0 4 0 4',
                        checked: true
                    }
                ]
            }, {
                xtype: 'fieldcontainer',
                defaultType: 'checkboxfield',
                layout: 'hbox', // hbox水平排列, hbox垂直排列
                fieldLabel: '確認狀態',
                items: [
                    {
                        boxLabel: '已確認',
                        name: 'T1QF9_1',
                        id: 'T1QF9_01',
                        inputValue: 'Y',
                        padding: '0 4 0 4',
                        checked: true
                    }, {
                        boxLabel: '待確認',
                        name: 'T1QF9_2',
                        id: 'T1QF9_02',
                        inputValue: 'N',
                        padding: '0 4 0 4',
                        checked: true
                    }
                ]
            }, {
                xtype: 'fieldcontainer',
                defaultType: 'checkboxfield',
                layout: 'hbox', // hbox水平排列, hbox垂直排列
                fieldLabel: '出庫狀態',
                items: [
                    {
                        boxLabel: '已出庫',
                        name: 'T1QF10_1',
                        id: 'T1QF10_01',
                        inputValue: 'Y',
                        padding: '0 4 0 4'
                    }, {
                        boxLabel: '待出庫',
                        name: 'T1QF10_2',
                        id: 'T1QF10_02',
                        inputValue: 'N',
                        padding: '0 4 0 4',
                        checked: true
                    }
                ]
                // --
                //    xtype: 'combo',
                //    fieldLabel: '申請單號',
                //    name: 'P0',
                //    id: 'P0',
                //    store: st_docno,
                //    queryMode: 'local',
                //    displayField: 'TEXT',
                //    valueField: 'VALUE'
                //}, {
                //    xtype: 'datefield',
                //    fieldLabel: '申請日期',
                //    name: 'D0',
                //    id: 'D0',
                //    vtype: 'dateRange',
                //    dateRange: { end: 'D1' },
                //    padding: '0 4 0 4'
                //}, {
                //    xtype: 'datefield',
                //    fieldLabel: '至',
                //    labelWidth: '10px',
                //    name: 'D1',
                //    id: 'D1',
                //    labelSeparator: '',
                //    vtype: 'dateRange',
                //    dateRange: { begin: 'D0' },
                //    padding: '0 4 0 4'
                //}, {
                //    xtype: 'combo',
                //    fieldLabel: '申請單狀態',
                //    name: 'P2',
                //    id: 'P2',
                //    store: st_Flowid,
                //    queryMode: 'local',
                //    displayField: 'COMBITEM',
                //    valueField: 'VALUE'
                //}, {
                //    xtype: 'combo',
                //    fieldLabel: '申請單分類',
                //    name: 'P3',
                //    id: 'P3',
                //    store: st_apply_kind,
                //    queryMode: 'local',
                //    displayField: 'TEXT',
                //    valueField: 'VALUE'
                //}, {
                //    xtype: 'combo',
                //    fieldLabel: '物料分類',
                //    name: 'P4',
                //    id: 'P4',
                //    store: st_matclass,
                //    queryMode: 'local',
                //    displayField: 'COMBITEM',
                //    valueField: 'VALUE'
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                T2Grid.down('#export').setDisabled(true);
                T1Load();
                msglabel('訊息區:');
            }
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                T1QueryForm.getForm().findField('T1QF1').setValue(""); // 開始揀貨日期
                T1QueryForm.getForm().findField('T1QF2').setValue(""); // 結束揀貨日期
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                msglabel('訊息區:');
            }
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'PICK_USERID', type: 'string' },
            { name: 'APPDEPT', type: 'string' },
            { name: 'APPDEPTNAME', type: 'string' },
            { name: 'APPNAME', type: 'string' },
            { name: 'ITEM_SUM', type: 'string' },
            { name: 'APPQTY_SUM', type: 'string' },
            { name: 'LOT_NO', type: 'string' },
            { name: 'ACT_PICK_QTY_SUM', type: 'string' },
            { name: 'DIFFQTY_SUM', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CD0010/AllM',
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
                    //p0: T1QueryForm.getForm().findField('T1QF0').getValue(), // combo,  date, 
                    //d0: T1QueryForm.getForm().findField('T1QF0').rawValue, // ?

                    wh_no: T1QueryForm.getForm().findField('T1QF0').getValue(), // combox
                    pick_date_start: T1QueryForm.getForm().findField('T1QF1').getValue(), // text
                    pick_date_end: T1QueryForm.getForm().findField('T1QF2').getValue(), // text
                    shopout_date_start: T1QueryForm.getForm().findField('T1QF3').getValue(), // text
                    shopout_date_end: T1QueryForm.getForm().findField('T1QF4').getValue(), // text
                    docno: T1QueryForm.getForm().findField('T1QF5').getValue(), // text
                    appdept: T1QueryForm.getForm().findField('T1QF12').getValue(),
                    mmcode: T1QueryForm.getForm().findField('T1QF6').getValue(), // text
                    pick_userid: T1QueryForm.getForm().findField('T1QF11').getValue(),
                    act_pick_userid: T1QueryForm.getForm().findField('T1QF7').getValue(), // combobox
                    has_appqty: getCheckboxYNValue(T1QueryForm, "T1QF8_1", "T1QF8_2"), // checkbox
                    has_confirmed: getCheckboxYNValue(T1QueryForm, "T1QF9_1", "T1QF9_2"), // checkbox
                    has_shopout: getCheckboxYNValue(T1QueryForm, "T1QF10_1", "T1QF10_2") // checkbox
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    // 使用範例 T1QueryForm.getForm().findField('T1QF8_1').getValue()!=""
    // getCheckboxYNValue(T1QueryForm, "T1QF8_1", "T1QF8_2") 回傳 [Y|N|YN|]
    function getCheckboxYNValue(objForm, yesObjName, noObjName) {
        if (
            objForm.getForm().findField(yesObjName).getValue() != "" &&
            objForm.getForm().findField(noObjName).getValue() != ""
        ) {
            return "YN";
        }
        else if (
            objForm.getForm().findField(yesObjName).getValue() != ""
        ) {
            return "Y";
        }
        else if (
            objForm.getForm().findField(noObjName).getValue() != ""
        ) {
            return "N";
        }
        return "";
    }
    function T1Load() {
        T1Tool.moveFirst();
        //viewport.down('#form').collapse();
    }

    function link2(url, text) {
        $("iframe#page-content").attr("src", url);
        $("#page-title").text("載入中...");
        $("#page-title-pre").text(text);
        $("#msglabel").text('');
    }
    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'print1', text: '已核發出帳報表', disabled: true, handler: function () {
                    
                    var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let docno = '';
                        //selection.map(item => {
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            docno += item.get('DOCNO') + ',';
                        })
                        v_DOCNO = docno;
                        showReport1();
                    }
                }
            },{
                itemId: 'link1', text: '分配揀貨', disabled: true, handler: function () {
                    parent.link2('/Form/Index/CD0009', '藥庫分配揀貨人員(CD0009)', true);
                }
            },
            {
                itemId: 'link2', text: '揀貨', disabled: true, handler: function () {
                    parent.link2('/Form/Mobile/CD0004', ' 點選申請單編號(PDA)(CD0004)', true);
                }
            },
            {
                itemId: 'link3', text: '出庫', disabled: true, handler: function () {
                    parent.link2('/Form/Mobile/CD0008', '裝箱出庫作業(CD0008)', true);
                }
            }
            //{
            //    itemId: 'add',text: '新增', handler: function () {
            //        T1Set = '/api/AB0003/CreateM'; 
            //        msglabel('訊息區:');
            //        setFormT1('I', '新增');
            //        TATabs.setActiveTab('Form');
            //    }
            //},
            //{
            //    itemId: 'edit', text: '修改', disabled: true, handler: function () {
            //        T1Set = '/api/AB0003/UpdateM';
            //        msglabel('訊息區:');
            //        setFormT1("U", '修改');
            //    }
            //}
            //, {
            //    itemId: 'delete', text: '刪除', disabled: true,
            //    handler: function () {
            //        var selection = T1Grid.getSelection();
            //        if (selection.length === 0) {
            //            Ext.Msg.alert('提醒', '請勾選項目');
            //        }
            //        else
            //        {
            //            let name = '';
            //            let docno = '';
            //            selection.map(item => {
            //                name += '「' + item.get('DOCNO') + '」<br>';
            //                docno += item.get('DOCNO') + ',';
            //            });
            //            Ext.MessageBox.confirm('刪除', '是否確定刪除申請單號?<br>' + name, function (btn, text) {
            //                if (btn === 'yes') {
            //                    Ext.Ajax.request({
            //                        url: '/api/AB0003/DeleteM',
            //                        method: reqVal_p,
            //                        params: {
            //                            DOCNO: docno
            //                        },
            //                        success: function (response) {
            //                            var data = Ext.decode(response.responseText);
            //                            if (data.success) {
            //                                Ext.MessageBox.alert('訊息', '刪除申請單號<br>' + name + '成功');
            //                                T1Grid.getSelectionModel().deselectAll();
            //                                T1Load();
            //                            }
            //                            else
            //                                Ext.MessageBox.alert('錯誤', data.msg);
            //                        },
            //                        failure: function (response) {
            //                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            //                        }
            //                    });
            //                }
            //            }
            //            );
            //        }
            //    }
            //},
            //{
            //    itemId: 'apply', text: '送核撥', disabled: true, handler: function () {
            //        var selection = T1Grid.getSelection();
            //        if (selection.length === 0) {
            //            Ext.Msg.alert('提醒', '請勾選項目');
            //        }
            //        else
            //        {
            //            let name = '';
            //            let docno = '';
            //            selection.map(item => {
            //                name += '「' + item.get('DOCNO') + '」<br>';
            //                docno += item.get('DOCNO') + ',';
            //            });
            //            Ext.MessageBox.confirm('送核撥', '是否確定送核撥？單號如下<br>' + name, function (btn, text) {
            //                if (btn === 'yes') {
            //                    Ext.Ajax.request({
            //                        url: '/api/AB0003/Apply',
            //                        method: reqVal_p,
            //                        params: {
            //                            DOCNO: docno
            //                        },
            //                        //async: true,
            //                        success: function (response) {
            //                            var data = Ext.decode(response.responseText);
            //                            if (data.success) {
            //                                Ext.MessageBox.alert('訊息', '送出成功');
            //                                T2Store.removeAll();
            //                                T1Grid.getSelectionModel().deselectAll();
            //                                T1Load();
            //                            }
            //                            else
            //                                Ext.MessageBox.alert('錯誤', data.msg);
            //                        },
            //                        failure: function (response) {
            //                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            //                        }
            //                    });
            //                }
            //            }
            //            );
            //        }
            //    }
            //},
            //{
            //    itemId: 'savepk', text: '套餐儲存', disabled: true, handler: function () {
            //        msglabel('訊息區:');
            //        viewport.down('#form').collapse();
            //        showWin3();
            //    }
            //}
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
            var r = Ext.create('WEBAPP.model.ME_DOCM'); // /Scripts/app/model/ME_DOCM.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("DOCNO");
            u.setreadonly(false);
            u.clearinvalid();

            f.findField('DOCNO_D').setValue('系統自編');
            f.findField('FLOWID_N').setValue('申請中');
            f.findField('APPTIME_T').setValue(st_getlogininfo.getAt(0).get('TODAY'));
            f.findField('FRWH_N').setValue(st_getlogininfo.getAt(0).get('CENTER_WHNAME'));
            f.findField('APP_NAME').setValue(st_getlogininfo.getAt(0).get('USERNAME'));
            f.findField('APPDEPT_NAME').setValue(st_getlogininfo.getAt(0).get('INIDNAME'));
            f.findField('FLOWID').setValue('1');
            f.findField('DOCTYPE').setValue('MR1');
            f.findField('APPLY_KIND').setValue('2');  //臨時申請
            f.findField('APPLY_KIND_N').setValue('臨時申請');
            f.findField('TOWH').setValue(st_towhcombo.getAt(0).get('VALUE'));
            f.findField('MAT_CLASS').setReadOnly(false);
            f.findField('TOWH').setReadOnly(false);
        }
        else {
            u = f.findField('DOCNO');
            f.findField("DOCNO").setValue(T1LastRec.data.DOCNO);
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
            items: [T1Tool]
        }],
        //selModel: {
        //    checkOnly: false,
        //    allowDeselect: true,
        //    injectCheckbox: 'first',
        //    mode: 'MULTI'
        //    //,
        //    //listeners: {
        //    //    'beforeselect': function (view, rec, index) {
        //    //        var MR3 = st_getlogininfo.getAt(0).get('MR3');
        //    //        var str_APPLY_KIND = rec.get('APPLY_KIND');
        //    //        var str_FLOWID = rec.get('FLOWID');
        //    //        //如為常態申請期間，常態申請單且申請單狀態(FLOWID)=1，checkbox可勾選
        //    //        if (MR3 == "Y" && str_APPLY_KIND == '1' && str_FLOWID == '1') {
        //    //            return true;
        //    //        }
        //    //        //如為臨時申請期間，臨時申請單且申請單狀態(FLOWID)=1，checkbox可勾選
        //    //        else if (MR3 == "N" && str_APPLY_KIND == '2' && str_FLOWID == '1') {
        //    //            return true;
        //    //        }
        //    //    }
        //    //}
        //},
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "申請單號碼", dataIndex: 'DOCNO',
            width: 150
        }, {
            text: "單據狀態", dataIndex: 'FLOWID_DES',
            width: 150
        }, {
            text: "申請部門", dataIndex: 'APPDEPT',
            width: 80
        }, {
            text: "申請部門名稱", dataIndex: 'APPDEPTNAME',
            width: 135
        }, {
            text: "申請人員", dataIndex: 'APPNAME',
            width: 80
        }, {
            text: "核撥品項數", dataIndex: 'ITEM_SUM',
            width: 80, style: 'text-align:left', align: 'right'
        }, {
            text: "核撥件數", dataIndex: 'APPQTY_SUM',
            width: 80, style: 'text-align:left', align: 'right'
        }, {
            text: "分配揀貨人員", dataIndex: 'PICK_USERID',
            width: 100
        }, {
            text: "揀貨批次", dataIndex: 'LOT_NO',
            width: 80, style: 'text-align:left', align: 'right'
        }, {
            text: "已揀品項數", dataIndex: 'ACT_PICK_ITEM_SUM',
            width: 80, style: 'text-align:left', align: 'right'
        }, {
            text: "已揀件數", dataIndex: 'ACT_PICK_QTY_SUM',
            width: 80, style: 'text-align:left', align: 'right'
        }, {
            text: "差異品項數", dataIndex: 'DIFFITEM_SUM',
            width: 80, style: 'text-align:left', align: 'right'
        }, {
            text: "差異件數", dataIndex: 'DIFFQTY_SUM',
            width: 80, style: 'text-align:left', align: 'right'
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

                T2Store.removeAll();
                T2Tool.onLoad();
                if (T1Rec > 0)
                    setFormT1a();
            }
        }
    });
    function setFormT1a() {
        //T1Grid.down('#edit').setDisabled(T1Rec === 0);
        //T1Grid.down('#delete').setDisabled(T1Rec === 0);
        //T1Grid.down('#apply').setDisabled(T1Rec === 0);
        //T1Grid.down('#savepk').setDisabled(T1Rec === 0);
        //T2Grid.down('#add').setDisabled(T1Rec === 0);
        //T2Grid.down('#getpk').setDisabled(T1Rec === 0);
        T1Grid.down('#print1').setDisabled(T1Rec === 0);
        T2Grid.down('#print2').setDisabled(T1Rec === 0);
        T2Grid.down('#print3').setDisabled(T1Rec === 0);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            // T3Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('DOCNO');
            //u.setReadOnly(true);
            //u.setFieldStyle('border: 0px');
            //u.setValue(T1LastRec.data.DOCNO);
            //T1QueryForm.getForm().findField('T1QF5').setValue(T1LastRec.data.DOCNO); // 點T1Grid時，把DOCNO帶到查詢條件
            DOCNO = T1LastRec.data.DOCNO;
            //f.findField('MAT_CLASS').setReadOnly(true);
            //f.findField('TOWH').setReadOnly(true);
            T1F1 = f.findField('DOCNO').getValue();
            //f.findField('DOCNO_D').setValue(T1F1);
            //T1F2 = f.findField('MAT_CLASS').getValue();
            //T1F3 = f.findField('FLOWID').getValue();
            //T1F4 = f.findField('TOWH').getValue();
            //T1F5 = f.findField('APPDEPT').getValue();
            //T1F6 = f.findField('APPDEPT_NAME').getValue();
            //T1F7 = f.findField('MAT_CLASS_N').getValue();
            T3Form.getForm().findField('T3P0').setValue(T1F1);
            T3Form.getForm().findField('T3P2').setValue('0');

            // 載入下拉式選單
            //st_pkdocno.load();
            //st_pknote.load();

            //if (T1F3 == '1') {
            //    T1Grid.down('#edit').setDisabled(false);
            //    T1Grid.down('#delete').setDisabled(false);
            //    T1Grid.down('#apply').setDisabled(false);
            //    T2Grid.down('#add').setDisabled(false);
            //    T2Grid.down('#getpk').setDisabled(false);
            //}
            //else {
            //T1Grid.down('#edit').setDisabled(true);
            //T1Grid.down('#delete').setDisabled(true);
            //T1Grid.down('#apply').setDisabled(true);
            //T2Grid.down('#add').setDisabled(true);
            //T2Grid.down('#getpk').setDisabled(true);
            //}
        }
        else {
            T1Form.getForm().reset();
            T1F1 = '';
            T1F2 = '';
            DOCNO = '';
        }
        T2Cleanup();
        T2Query.getForm().reset();
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
            //}, {
            //    name: 'APPDEPT',
            //    xtype: 'hidden'
        }, {
            name: 'USEID',
            xtype: 'hidden'
        }, {
            name: 'USEDEPT',
            xtype: 'hidden'
        }, {
            name: 'APPTIME',
            xtype: 'hidden'
            //}, {
            //    name: 'DOCNO',
            //    xtype: 'hidden'
            }, {
                name: 'USE_BOX_QTY',
                xtype: 'hidden'
            }, {
            name: 'MAT_CLASS_N',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield', fieldLabel: '申請單號碼', name: 'DOCNO'
        }, {
            xtype: 'displayfield', fieldLabel: '單據狀態', name: 'FLOWID_DES'
        }, {
            xtype: 'displayfield', fieldLabel: '申請部門', name: 'APPDEPT'
        }, {
            xtype: 'displayfield', fieldLabel: '申請部門名稱', name: 'APPDEPTNAME'
        }, {
            xtype: 'displayfield', fieldLabel: '核撥品項數', name: 'ITEM_SUM'
        }, {
            xtype: 'displayfield', fieldLabel: '核撥件數', name: 'APPQTY_SUM'
        }, {
            xtype: 'displayfield', fieldLabel: '分配揀貨人員', name: 'PICK_USERID'
        }, {
            xtype: 'displayfield', fieldLabel: '揀貨批次', name: 'LOT_NO'
        }, {
            xtype: 'displayfield', fieldLabel: '已揀品項數', name: 'ACT_PICK_ITEM_SUM'
        }, {
            xtype: 'displayfield', fieldLabel: '已揀件數', name: 'ACT_PICK_QTY_SUM'
        }, {
            xtype: 'displayfield', fieldLabel: '差異品項數', name: 'DIFFITEM_SUM'
        }, {
            xtype: 'displayfield', fieldLabel: '差異件數', name: 'DIFFQTY_SUM'


            //    xtype: 'displayfield',
            //    fieldLabel: '類別',
            //    name: 'APPLY_KIND_N'
            //}, {
            //    xtype: 'displayfield',
            //    fieldLabel: '狀態',
            //    name: 'FLOWID_N'
            //}, {
            //    xtype: 'displayfield',
            //    fieldLabel: '申請人員',
            //    name: 'APP_NAME'
            //}, {
            //    xtype: 'displayfield',
            //    fieldLabel: '申請部門',
            //    name: 'APPDEPT_NAME'
            //}, {
            //    xtype: 'displayfield',
            //    fieldLabel: '申請時間',
            //    name: 'APPTIME_T'
            //}, {
            //    xtype: 'displayfield',
            //    fieldLabel: '出庫庫房',
            //    name: 'FRWH_N'

            //}, {
            //    xtype: 'combo',
            //    fieldLabel: '入庫庫房',
            //    name: 'TOWH',
            //    store: st_towhcombo,
            //    queryMode: 'local',
            //    displayField: 'COMBITEM',
            //    valueField: 'VALUE',
            //    anyMatch: true,
            //    readOnly: true,
            //    allowBlank: false, // 欄位為必填
            //    typeAhead: true,
            //    forceSelection: true,
            //    queryMode: 'local',
            //    triggerAction: 'all',
            //    fieldCls: 'required'
            //}, {
            //    xtype: 'combo',
            //    fieldLabel: '物料分類',
            //    name: 'MAT_CLASS',
            //    store: st_matclass,
            //    queryMode: 'local',
            //    displayField: 'COMBITEM',
            //    valueField: 'VALUE',
            //    anyMatch: true,
            //    readOnly: true,
            //    allowBlank: false, // 欄位為必填
            //    typeAhead: true,
            //    forceSelection: true,
            //    queryMode: 'local',
            //    triggerAction: 'all',
            //    fieldCls: 'required'
            //}, {
            //    xtype: 'textareafield',
            //    fieldLabel: '備註',
            //    name: 'APPLY_NOTE',
            //    enforceMaxLength: true,
            //    maxLength: 100,
            //    height: 200,
            //    readOnly: true
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    /*
                                        if (this.up('form').getForm().findField('WH_NO').getValue() == ''
                                        ) //&& this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
                                            Ext.Msg.alert('提醒', '至少需輸入');
                                        else {*/
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );

                }
                /*else
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');*/
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
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T1QueryForm.getForm().reset();
                            var v = action.result.etts[0];
                            T1QueryForm.getForm().findField('P0').setValue(v.DOCNO);
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
        T1QueryForm.getForm().findField('P2').setValue("1");
        TATabs.setActiveTab('Query');
    }

    //Detail
    var T2Rec = 0;
    var T2LastRec = null;

    var T2QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0003/GetMMCodeDocd', //指定查詢的Controller路徑
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
            { name: 'SEQ', type: 'string' }, // 01.項次
            { name: 'MMCODE', type: 'string' }, // 02.院內碼
            { name: 'MMNAME_C', type: 'string' }, // 03.中文品名
            { name: 'MMNAME_E', type: 'string' }, // 04.英文品名
            { name: 'APPQTY', type: 'string' }, // 05.申請數
            { name: 'BASE_UNIT', type: 'string' }, // 06.撥補單位
            { name: 'STORE_LOC', type: 'string' }, // 07.儲位
            { name: 'ACT_PICK_USERNAME', type: 'string' }, // 08.揀貨人員
            { name: 'ACT_PICK_QTY', type: 'string' }, // 09.揀貨數
            { name: 'HAS_CONFIRMED', type: 'string' }, // 10.已確認
            { name: 'BOXNO', type: 'string' }, // 11.物流箱號
            { name: 'HAS_SHIPOUT', type: 'string' } // 12.已出庫
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'SEQ', direction: '' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CD0010/AllD',
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

                    WH_NO: T1QueryForm.getForm().findField('T1QF0').getValue(), // combox
                    PICK_DATE_START: T1QueryForm.getForm().findField('T1QF1').getValue(), // text
                    PICK_DATE_END: T1QueryForm.getForm().findField('T1QF2').getValue(), // text
                    SHOPOUT_DATE_START: T1QueryForm.getForm().findField('T1QF3').getValue(), // text
                    SHOPOUT_DATE_END: T1QueryForm.getForm().findField('T1QF4').getValue(), // text
                    DOCNO: DOCNO, // T1QueryForm.getForm().findField('T1QF5').getValue(), // text  
                    MMCODE: T1QueryForm.getForm().findField('T1QF6').getValue(),        // T1QueryForm.getForm().findField('T1QF6').getValue(), // text
                    ACT_PICK_USERID: T1QueryForm.getForm().findField('T1QF7').getValue(), // combobox
                    HAS_APPQTY: getCheckboxYNValue(T1QueryForm, "T1QF8_1", "T1QF8_2"), // checkbox
                    HAS_CONFIRMED: getCheckboxYNValue(T1QueryForm, "T1QF9_1", "T1QF9_2"), // checkbox
                    HAS_SHOPOUT: getCheckboxYNValue(T1QueryForm, "T1QF10_1", "T1QF10_2") // checkbox

                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T2Load() {
        try {
            T2Tool.moveFirst();
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
        queryUrl: '/api/AB0003/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'M_CONTPRICE', 'AVG_PRICE', 'INV_QTY', 'AVG_APLQTY', 'HIGH_QTY', 'TOT_APVQTY'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('MAT_CLASS').getValue(),
                p2: T1Form.getForm().findField('TOWH').getValue(),
                p3: T1Form.getForm().findField('DOCNO').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                T2Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                T2Form.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
                T2Form.getForm().findField('BASE_UNIT').setValue(r.get('BASE_UNIT'));
                T2Form.getForm().findField('M_CONTPRICE').setValue(r.get('M_CONTPRICE'));
                T2Form.getForm().findField('AVG_PRICE').setValue(r.get('AVG_PRICE'));
                T2Form.getForm().findField('INV_QTY').setValue(r.get('INV_QTY'));
                T2Form.getForm().findField('AVG_APLQTY').setValue(r.get('AVG_APLQTY'));
                T2Form.getForm().findField('HIGH_QTY').setValue(r.get('HIGH_QTY'));
                T2Form.getForm().findField('TOT_APVQTY').setValue(r.get('TOT_APVQTY'));
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
            msgTarget: 'side'
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            //    name: 'DOCNO',
            //    xtype: 'hidden'
            //}, {
            //    name: 'STAT',
            //    xtype: 'hidden'
            //}, {
            //    xtype: 'container',
            //    layout: 'hbox',
            //    //padding: '0 7 7 0',
            //    items: [
            //        T2FormMMCode,
            //        {
            //            xtype: 'button',
            //            itemId: 'btnMmcode',
            //            iconCls: 'TRASearch',
            //            handler: function () {
            //                var f = T2Form.getForm();
            //                if (!f.findField("MMCODE").readOnly) {
            //                    popMmcodeForm(viewport, '/api/AB0003/GetMmcode', {
            //                        MMCODE: f.findField("MMCODE").getValue(),
            //                        MAT_CLASS: T1Form.getForm().findField('MAT_CLASS').getValue(),
            //                        WH_NO: T1Form.getForm().findField('TOWH').getValue()
            //                    }, setMmcode);
            //                }
            //            }
            //        },

            //    ]
            //}, {
            xtype: 'displayfield', fieldLabel: '項次', name: 'SEQ'
        }, {
            xtype: 'displayfield', fieldLabel: '院內碼', name: 'MMCODE'
        }, {
            xtype: 'displayfield', fieldLabel: '中文品名', name: 'MMNAME_C'
        }, {
            xtype: 'displayfield', fieldLabel: '英文品名', name: 'MMNAME_E'
        }, {
            xtype: 'displayfield', fieldLabel: '核撥數', name: 'APPQTY'
        }, {
            xtype: 'displayfield', fieldLabel: '撥補單位', name: 'BASE_UNIT'
        }, {
            xtype: 'displayfield', fieldLabel: '儲位', name: 'STORE_LOC'
        }, {
            xtype: 'displayfield', fieldLabel: '核撥日', name: 'APVDATE'
        }, {
            xtype: 'displayfield', fieldLabel: '實際揀貨人員', name: 'ACT_PICK_USERNAME'
        }, {
            xtype: 'displayfield', fieldLabel: '已揀件數', name: 'ACT_PICK_QTY'
        }, {
            xtype: 'displayfield', fieldLabel: '差異件數', name: 'DIFFQTY'
        }, {
            xtype: 'displayfield', fieldLabel: '已確認', name: 'HAS_CONFIRMED'
        }, {
            xtype: 'displayfield', fieldLabel: '物流箱號', name: 'BOXNO'
        }, {
            xtype: 'displayfield', fieldLabel: '已出庫', name: 'HAS_SHIPOUT'
        }],

        buttons: [
            //{
            //    itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
            //        var isSub = false;
            //        if (this.up('form').getForm().findField('APPQTY').getValue() == '0')
            //        {
            //            Ext.Msg.alert('提醒', '申請數量不可為0');
            //            isSub = false;
            //        }
            //        else {
            //            if (this.up('form').getForm().findField('HIGH_QTY').getValue() != null) {
            //                var highqty = Number(this.up('form').getForm().findField('HIGH_QTY').getValue());
            //                var tot_apvqty = Number(this.up('form').getForm().findField('TOT_APVQTY').getValue());
            //                var appqty = Number(this.up('form').getForm().findField('APPQTY').getValue());
            //                if ((tot_apvqty + appqty) > highqty && this.up('form').getForm().findField('GTAPL_RESON').getValue() == null) {
            //                    Ext.Msg.alert('提醒', '超量申請，請敘明原因!');
            //                    isSub = false;
            //                }
            //                else {
            //                    isSub = true;
            //                }
            //            }
            //            else {
            //                isSub = true;
            //            }
            //        }
            //        if (isSub) {
            //            var confirmSubmit = viewport.down('#form').title.substring(0, 2);
            //            Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
            //                if (btn === 'yes') {
            //                    T2Submit();
            //                }
            //            }
            //            );
            //        }
            //    }
            //}, {
            //    itemId: 'T2Cancel', text: '取消', hidden: true, handler: T2Cleanup
            //}
        ]
    });
    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T11Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
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
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        //f.findField('MMCODE').setReadOnly(true);
        //f.findField('APPQTY').setReadOnly(true);
        //f.findField('APLYITEM_NOTE').setReadOnly(true);
        //T2Form.down('#T2Cancel').hide();
        //T2Form.down('#T2Submit').hide();
        //T2Form.down('#btnMmcode').setVisible(false);
        viewport.down('#form').setTitle('瀏覽');
        setFormT2a();
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [

            //{
            //    itemId: 'edit', text: '修改', disabled: true, handler: function () {
            //        T1Set = '/api/AB0003/UpdateM';
            //        msglabel('訊息區:');
            //        setFormT1("U", '修改');
            //    }
            //}
            {
                itemId: 'print2', text: '尚未出庫品項報表', disabled: true, handler: function () {
                    showReport2();
                }
            },
            {
                itemId: 'print3', text: '出庫簽收報表', disabled: true, handler: function () {
                    setT3P1();
                    T3Form.getForm().findField('T3P2').setValue('0');
                    showWin3();
                }
            },
            {
                //xtype: 'button',
                itemId: 'export',
                text: '匯出', //T1Query
                disabled: true,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'wh_no', value: T1QueryForm.getForm().findField('T1QF0').getValue() }); // combox
                    p.push({ name: 'pick_date_start', value: T1QueryForm.getForm().findField('T1QF1').getValue() }); // text
                    p.push({ name: 'pick_date_end', value: T1QueryForm.getForm().findField('T1QF2').getValue() }); // text
                    p.push({ name: 'shopout_date_start', value: T1QueryForm.getForm().findField('T1QF3').getValue() }); // text
                    p.push({ name: 'shopout_date_end', value: T1QueryForm.getForm().findField('T1QF4').getValue() }); // text
                    p.push({ name: 'docno', value: T1LastRec.data.DOCNO }); // text
                    p.push({ name: 'mmcode', value: T1QueryForm.getForm().findField('T1QF6').getValue() }); // text
                    p.push({ name: 'act_pick_userid', value: T1QueryForm.getForm().findField('T1QF7').getValue() }); // combobox
                    p.push({ name: 'has_appqty', value: getCheckboxYNValue(T1QueryForm, "T1QF8_1", "T1QF8_2") }); // checkbox
                    p.push({ name: 'has_confirmed', value: getCheckboxYNValue(T1QueryForm, "T1QF9_1", "T1QF9_2") }); // checkbox
                    p.push({ name: 'has_shopout', value: getCheckboxYNValue(T1QueryForm, "T1QF10_1", "T1QF10_2") }); // checkbox
                    //PostForm('../../../api/AA0064/Excel', p);
                    PostForm('../../../api/CD0010/Excel', p);
                    msglabel('訊息區:匯出完成');
                }
            }
            //{
            //    itemId: 'add', text: '新增', disabled: true, handler: function () {
            //        T11Set = '../../../api/AB0003/CreateD';
            //        setFormT2('I', '新增');
            //    }
            //},
            //{
            //    itemId: 'edit', text: '修改', disabled: true, handler: function () {
            //        T11Set = '../../../api/AB0003/UpdateD';
            //        setFormT2("U", '修改');
            //    }
            //},
            //{
            //    itemId: 'delete', text: '刪除', disabled: true,
            //    handler: function () {
            //        var selection = T2Grid.getSelection();
            //        if (selection.length === 0) {
            //            Ext.Msg.alert('提醒', '請勾選項目');
            //        }
            //        else
            //        {
            //            let name = '';
            //            let docno = '';
            //            let seq = '';
            //            selection.map(item => {
            //                name += '「' + item.get('SEQ') + '」<br>';
            //                docno += item.get('DOCNO') + ',';
            //                seq += item.get('SEQ') + ',';
            //            });
            //            Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
            //                if (btn === 'yes') {
            //                    Ext.Ajax.request({
            //                        url: '/api/AB0003/DeleteD',
            //                        method: reqVal_p,
            //                        params: {
            //                            DOCNO: docno,
            //                            SEQ: seq
            //                        },
            //                        success: function (response) {
            //                            var data = Ext.decode(response.responseText);
            //                            if (data.success) {
            //                                Ext.MessageBox.alert('訊息', '刪除項次<br>' + name + '成功');
            //                                //T2Store.removeAll();
            //                                T2Grid.getSelectionModel().deselectAll();
            //                                T2Load();
            //                                //Ext.getCmp('btnSubmit').setDisabled(true);
            //                            }
            //                        },
            //                        failure: function (response) {
            //                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            //                        }
            //                    });
            //                }
            //            }
            //            );
            //        }
            //    }
            //},
            //{
            //    itemId: 'getpk', text: '套餐轉入', disabled: true, handler: function () {
            //        msglabel('訊息區:');
            //        viewport.down('#form').collapse();
            //        showWin4();
            //    }
            //}
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
            //var r = Ext.create('T2Model');
            var r = Ext.create('WEBAPP.model.ME_DOCD');
            T2Form.loadRecord(r);
            f2.findField('DOCNO').setValue(T1F1);
            u = f2.findField("MMCODE");
            f2.findField('MMCODE').setReadOnly(false);
            T2Form.down('#btnMmcode').setVisible(true);

            //u.setReadOnly(false);
        }
        else {
            u = f2.findField('APPQTY');
        }

        f2.findField('x').setValue(x);
        f2.findField('STAT').setValue('1');
        f2.findField('APPQTY').setReadOnly(false);
        f2.findField('APLYITEM_NOTE').setReadOnly(false);
        f2.findField('GTAPL_RESON').setReadOnly(false);
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
        dockedItems: [
            {
                //    dock: 'top',
                //    xtype: 'toolbar',
                //    layout: 'fit',
                //    items: [T2Query]
                //}, {
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
            text: "項次", dataIndex: 'SEQ', width: 40, sortable: true,
            style: 'text-align:left', align: 'right'
        }, {
            text: "院內碼", dataIndex: 'MMCODE', width: 70, sortable: true
        }, {
            text: "中文品名", dataIndex: 'MMNAME_C', width: 160, sortable: true
        }, {
            text: "英文品名", dataIndex: 'MMNAME_E', width: 200, sortable: true
        }, {
            text: "核撥數", dataIndex: 'APPQTY', width: 60, sortable: true,
            style: 'text-align:left', align: 'right'
        }, {
            text: "撥補單位", dataIndex: 'BASE_UNIT', width: 70, sortable: true
        }, {
            text: "儲位", dataIndex: 'STORE_LOC', width: 80, sortable: true
        }, {
            text: "核撥日", dataIndex: 'APVDATE', width: 80, sortable: true
        }, {
            text: "實際揀貨人員", dataIndex: 'ACT_PICK_USERNAME', width: 100, sortable: true
        }, {
            text: "已揀件數", dataIndex: 'ACT_PICK_QTY', width: 70, sortable: true,
            style: 'text-align:left', align: 'right'
        }, {
            text: "差異件數", dataIndex: 'DIFFQTY', width: 70, sortable: true,
            style: 'text-align:left', align: 'right'
        }, {
            text: "已確認", dataIndex: 'HAS_CONFIRMED', width: 50, sortable: true
        }, {
            text: "物流箱號", dataIndex: 'BOXNO', width: 60, sortable: true
        }, {
            text: "已出庫", dataIndex: 'HAS_SHIPOUT', width: 50, sortable: true
        },
        //{ 
        //    text: "院內碼",
        //    dataIndex: 'MMCODE',
        //    width: 100,
        //    sortable: true
        //}, {
        //    text: "中文品名",
        //    dataIndex: 'MMNAME_C',
        //    width: 120,
        //    sortable: true
        //}, {
        //    text: "英文品名",
        //    dataIndex: 'MMNAME_E',
        //    width: 150,
        //    sortable: true
        //} ,{
        //    text: "單位",
        //    dataIndex: 'BASE_UNIT',
        //    width: 50,
        //    sortable: true
        //}, {
        //    text: "庫存單價",
        //        dataIndex: 'AVG_PRICE',
        //        style: 'text-align:left',
        //    width: 80, align: 'right'
        //}, {
        //    text: "庫存數量",
        //        dataIndex: 'INV_QTY',
        //        style: 'text-align:left',
        //    width: 80, align: 'right'
        //}, {
        //    text: "平均申請數量",
        //        dataIndex: 'AVG_APLQTY',
        //        style: 'text-align:left',
        //    width: 100, align: 'right'
        //}, {
        //    text: "申請數量",
        //        dataIndex: 'APPQTY',
        //        style: 'text-align:left',
        //    width: 80, align: 'right'
        //},
        {
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
                //;
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });
    function setFormT2a() {
        //;
        //T2Grid.down('#edit').setDisabled(T2Rec === 0);
        //T2Grid.down('#delete').setDisabled(T2Rec === 0);
        T2Grid.down('#export').setDisabled(T2Rec !== 0);
        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
            f.findField('x').setValue('U');
            //f.findField('DATA_SEQ_O').setValue(T2LastRec.get('DATA_SEQ'));
            //var u = f.findField('ID');
            //u.setReadOnly(true);
            //u.setFieldStyle('border: 0px');
            //if (T1F3 === '1') {
            //    T2Grid.down('#edit').setDisabled(false);
            //    T2Grid.down('#delete').setDisabled(false);
            //}
            //else {
            //    T2Grid.down('#edit').setDisabled(true);
            //    T2Grid.down('#delete').setDisabled(true);
            //}
        }
        else {
            T2Form.getForm().reset();
        }
    }

    var T3Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        autoScroll: true,
        items: [
            {
                xtype: 'displayfield',
                fieldLabel: '申請單號', 
                name: 'T3P0',
                submitValue: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '已裝箱數',
                name: 'T3P1',
                submitValue: true
            },{
                xtype: 'textfield',
                fieldLabel: '本次裝箱數',
                labelAlign: 'right',
                name: 'T3P2',
                padding: '0 4 0 4',
                width: 150
            }
        ],
        buttons: [{
            itemId: 'T3print', text: '確定', handler: function () {
                Ext.Ajax.request({
                    url: '/api/CD0010/UpdateBoxqty',
                    method: reqVal_p,
                    params: {
                        DOCNO: T1F1,
                        QTY: T3Form.getForm().findField('T3P2').getValue(),
                        WH_NO: T1QueryForm.getForm().findField('T1QF0').getValue()
                    },
                    //async: true,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            hideWin3();
                            showReport3();
                        }
                        else
                            Ext.MessageBox.alert('錯誤', data.msg);
                    },
                    failure: function (response) {
                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                    }
                });
            }
        },
        {
            itemId: 'T3cancel', text: '取消', handler: hideWin3
        }
        ]
    });

    Ext.define('T4Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'APVTIME', type: 'string' },
            { name: 'APVID', type: 'string' },
            { name: 'ACKQTY', type: 'string' },
            { name: 'ACKID', type: 'string' },
            { name: 'ACKTIME', type: 'string' },
            { name: 'STAT', type: 'string' },
            { name: 'RSEQ', type: 'string' },
            { name: 'EXPT_DISTQTY', type: 'string' },
            { name: 'PICK_QTY', type: 'string' },
            { name: 'PICK_USER', type: 'string' },
            { name: 'PICK_TIME', type: 'string' },
            { name: 'APLYITEM_NOTE', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'AVG_PRICE', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'SAFE_QTY', type: 'string' }
        ]
    });
    var T4Store = Ext.create('Ext.data.Store', {
        model: 'T4Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0003/GetPackD',
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
                    p0: T4Query.getForm().findField('P0').getValue(),
                    p1: T4Query.getForm().findField('P1').getValue(),
                    p2: T1F2,
                    p3: T1F5
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    })

    function T4Load() {
        T4Store.load({
            params: {
                start: 0
            }
        });
    }
    var T4Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [{
            xtype: 'combo',
            fieldLabel: '套餐單號',
            name: 'P0',
            store: st_pkdocno,
            queryMode: 'local',
            displayField: 'TEXT',
            valueField: 'VALUE',
            labelAlign: 'right',
            listeners: {
                select: function (c, r, eo) {
                    Ext.getCmp('T4btn1').setDisabled(false);
                    Ext.getCmp('T4btn2').setDisabled(false);
                }
            }
        }, {
            xtype: 'combo',
            fieldLabel: '套餐說明',
            name: 'P1',
            store: st_pknote,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            labelAlign: 'right',
            width: '500px',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            anyMatch: true,
            listeners: {
                select: function (c, r, eo) {
                    Ext.getCmp('T4btn1').setDisabled(false);
                    Ext.getCmp('T4btn2').setDisabled(false);
                }
            }
        }, {
            xtype: 'button',
            text: '查詢',
            id: 'T4btn1',
            disabled: true,
            handler: function () {
                T4Load();
                msglabel('訊息區:');
            }
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus();
            }
        }, {
            xtype: 'button', text: '轉入',
            id: 'T4btn2',
            disabled: true,
            handler: function () {
                var selection = T4Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                }
                else {
                    //let docno = '';
                    //let mmcode = '';
                    //let appqty = '';
                    //selection.map(item => {
                    //    name += '「' + item.get('MMCODE') + '」<br>';
                    //    docno += T1F1 + ',';
                    //    mmcode += item.get('MMCODE') + ',';
                    //    appqty += item.get('APPQTY') + ',';
                    //});
                    Ext.MessageBox.confirm('轉入', '是否確定轉入？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AB0003/InsFromPk',
                                method: reqVal_p,
                                params: {
                                    DOCNO: docno,
                                    MMCODE: mmcode,
                                    APPQTY: appqty
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        Ext.MessageBox.alert('訊息', '轉入成功');
                                        hideWin4();
                                        //T2Store.removeAll();
                                        T2Grid.getSelectionModel().deselectAll();
                                        T2Load();
                                    }
                                    else {
                                        hideWin4();
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                    }
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    hideWin4();
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

    function T4Cleanup() {
        T4Query.getForm().reset();
        T4Load();
        msglabel('訊息區:');
        Ext.getCmp('T4btn1').setDisabled(true);
        Ext.getCmp('T4btn2').setDisabled(true);
    }
    var T4Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T4Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T4Query]
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
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50,
            sortable: true
        }, {
            text: "庫存單價",
            dataIndex: 'AVG_PRICE',
            width: 80, align: 'right'
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            header: "",
            flex: 1
        }
        ]
    });

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
                        //items: [TATabs]
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

    var winActWidth = viewport.width - 10;
    var winActHeight = viewport.height - 10;

    var win3;
    var winActWidth3 = 300;
    var winActHeight3 = 200;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: '列印',
            closeAction: 'hide',
            width: winActWidth3,
            height: winActHeight3,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T3Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth3 > 0) ? winActWidth3 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight3 > 0) ? winActHeight3 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth3 = width;
                    winActHeight3 = height;
                }
            }
        });
    }
    function showWin3() {
        if (win3.hidden) {
            win3.show();
        }
    }
    function hideWin3() {
        if (!win3.hidden) {
            win3.hide();
        }
    }
    var win4;
    if (!win4) {
        win4 = Ext.widget('window', {
            title: '套餐',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T4Grid],
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

    function showWin4() {
        if (win4.hidden) {
            T4Cleanup();
            win4.show();
        }
    }
    function hideWin4() {
        if (!win4.hidden) {
            win4.hide();
            T4Cleanup();
            //viewport.down('#form').collapse();
        }
    }
    //T1Load(); // 進入畫面時自動載入一次資料
    T1QueryForm.getForm().findField('T1QF0').focus();
    // T1QueryForm.getForm().findField('P2').setValue("1"); 

    function setT1Btn() {
        Ext.Ajax.request({
            url: '/api/CD0010/GetUserPh1s',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == "Y") {
                        T1Grid.down('#link1').setDisabled(false);
                        T1Grid.down('#link2').setDisabled(false);
                        T1Grid.down('#link3').setDisabled(false);
                    }
                    else {
                        T1Grid.down('#link1').setDisabled(true);
                        T1Grid.down('#link2').setDisabled(true);
                        T1Grid.down('#link3').setDisabled(true);
                    }
                }
            },
            failure: function (response) {
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
    }
    setT1Btn();

    function showReport1() {
        if (!win1) {
            var np = {
                p0: T1QueryForm.getForm().findField('T1QF0').getValue(),
                p1: T1QueryForm.getForm().findField('T1QF1').rawValue,
                p2: T1QueryForm.getForm().findField('T1QF2').rawValue,
                p3: v_DOCNO
            };
            var winform1 = Ext.create('Ext.form.Panel', {
                id: 'iframeReport1',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl1 + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win1 = GetPopWin(viewport, winform1, '', viewport.width - 20, viewport.height - 20);
        }
        win1.show();
    }
    function showReport2() {
        if (!win2) {
            var np = {
                p0: T1QueryForm.getForm().findField('T1QF0').getValue(),
                p1: T1QueryForm.getForm().findField('T1QF1').rawValue,
                p2: T1QueryForm.getForm().findField('T1QF2').rawValue,
                p3: T1F1
            };
            var winform2 = Ext.create('Ext.form.Panel', {
                id: 'iframeReport2',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl2 + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win2 = GetPopWin(viewport, winform2, '', viewport.width - 20, viewport.height - 20);
        }
        win2.show();
    }
    function showReport3() {
        if (!win3) {
            var np = {
                p0: T1QueryForm.getForm().findField('T1QF0').getValue(),
                p1: T1QueryForm.getForm().findField('T1QF1').rawValue,
                p2: T1QueryForm.getForm().findField('T1QF2').rawValue,
                p3: T1F1,
                p4: T3Form.getForm().findField('T3P2').getValue()
            };
            var winform3 = Ext.create('Ext.form.Panel', {
                id: 'iframeReport3',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl3 + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '&p4=' + np.p4 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win3 = GetPopWin(viewport, winform3, '', viewport.width - 20, viewport.height - 20);
        }
        win3.show();
    }

    function setT3P1() {
        Ext.Ajax.request({
            url: '/api/CD0010/GetUse_box_qty',
            method: reqVal_p,
            params: {
                DOCNO: T1F1,
                WH_NO: T1QueryForm.getForm().findField('T1QF0').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T3Form.getForm().findField('T3P1').setValue(data.msg);
                }
            },
            failure: function (response) {
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
    }

});
