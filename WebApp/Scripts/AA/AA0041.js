Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

//如要使用widget才需要require
//Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.MMCodeCombo']);

Ext.onReady(function () {
    Ext.tip.QuickTipManager.init();

    var T1Create = '/api/AA0041/T1Create';
    var T1Update = '/api/AA0041/T1Update';
    var T1Delete = '/api/AA0041/T1Delete';
    var T1Exp = '/api/AA0041/T1Excel';
    var T1Set = T1Update; //T1Set預設為T1Update
    var T2Create = '/api/AA0041/T2Create';
    var T2Update = '/api/AA0041/T2Update';
    var T2Delete = '/api/AA0041/T2Delete';
    var T2Set = T2Update; //T2Set預設為T2Update
    var T3Get = '/api/AA0041/T3All';
    var GrpComboGet = '/api/AA0041/GetGrpCombo';
    var T3Del = '/api/AA0041/T3Delete';
    var T31Get = '/api/AA0041/T31All';
    var T31Add = '/api/AA0041/T31Add';
    //var T1Name = "責任中心基本檔";
    var T1Name = '';

    var T3Q_GRP_NO = ''; // 上一次查詢的GRP_NO

    var T1Rec = 0;
    var T1LastRec = null;
    var T2Rec = 0;
    var T2LastRec = null;
    var T3Rec = 0;
    var T3LastRec = null;
    var T31Rec = 0;
    var T31LastRec = null;

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var parBtnVis = Ext.getUrlParam('parBtnVis');

    var GrpQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setGrpComboData() {
        Ext.Ajax.request({
            url: GrpComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    GrpQueryStore.removeAll();
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            GrpQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 95,
            labelAlign: 'right'
        },
        border: false,
        items: [{

            fieldLabel: '責任中心代碼',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 200,
            padding: '0 4 0 4'

        }, {
            fieldLabel: '責任中心名稱',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 20,
            padding: '0 4 0 4'
        }, {
            xtype: 'button',
            text: '查詢',
            handler: T1Load
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus();
                msglabel('');
            }
        }]
    });

    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 95,
            labelAlign: 'right'
        },
        border: false,
        items: [{
            fieldLabel: '歸戶代碼',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 200,
            padding: '0 4 0 4'

        }, {
            xtype: 'button',
            text: '查詢',
            handler: T2Load
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus();
                msglabel('');
            }
        }]
    });

    var T3Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 70,
            labelAlign: 'right'
        },
        border: false,
        items: [{
            xtype: 'combo',
            fieldLabel: '歸戶代碼',
            name: 'P0',
            padding: '0 4 0 4',
            width: 280,
            store: GrpQueryStore,
            displayField: 'TEXT',
            valueField: 'VALUE',
            queryMode: 'local',
            anyMatch: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'button',
            text: '查詢',
            handler: T3Load
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus();
                msglabel('');
            }
        }]
    });

    var T31Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 95,
            labelAlign: 'right'
        },
        border: false,
        items: [{
            fieldLabel: '責任中心代碼',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 6,
            padding: '0 4 0 4'

        }, {
            fieldLabel: '責任中心名稱',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 30,
            padding: '0 4 0 4'

        }, {
            xtype: 'button',
            itemId: 'T31Qbtn',
            text: '查詢',
            disabled: true,
            handler: T31Load
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus();
                msglabel('');
            }
        }]
    });

    var T1Export = Ext.widget({
        xtype: 'form',
        defaultType: 'textfield',
        standardSubmit: true,
        url: T1Exp,
        items: [{
            name: 'sort'
        }, {
            name: 'p0'
        }, {
            name: 'p1'
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.UrInid', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {
                T1Tool.down('#export').setDisabled(store.getCount() == 0);
            }
        }
    });
    var T2Store = Ext.create('WEBAPP.store.UrInidGrp', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T2Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {
                
            }
        }
    });

    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: ['GRP_NO', 'INID', 'INID_NAME']
    });
    var T3Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T3Model',
        pageSize: 20,
        remoteSort: true,

        sorters: [{ property: 'GRP_NO', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var f = T3Query.getForm();
                // 載入前將查詢條件代入參數
                var np = {
                    P0: f.findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function () {
                T3GridShare.selectarray = Array();
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: T3Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });

    Ext.define('T31Model', {
        extend: 'Ext.data.Model',
        fields: ['INID', 'INID_NAME']
    });
    var T31Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T31Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'INID', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var f = T31Query.getForm();
                // 載入前將查詢條件代入參數
                var np = {
                    P0: T3Query.getForm().findField('P0').getValue(),
                    P1: f.findField('P0').getValue(),
                    P2: f.findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function () {
                T31GridShare.selectarray = Array();
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: T31Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
        msglabel('');
    }

    function T2Load() {
        T2Tool.moveFirst();
        msglabel('');
    }

    function T3Load() {
        if (T3Query.isValid())
        {
            T3GridShare.selectarray = Array();
            T31GridShare.selectarray = Array();
            T31Store.removeAll();
            T3Tool.moveFirst();
            T31Query.down('#T31Qbtn').setDisabled(false);
            T3Q_GRP_NO = T3Query.getForm().findField('P0').getValue();
        }
        else
        {
            Ext.Msg.alert('訊息', '查詢條件的歸戶代碼為必填');
            T31Query.down('#T31Qbtn').setDisabled(true);
        } 
        msglabel('');
    }
    function T31Load() {
        if (T3Query.isValid())
        {
            T31GridShare.selectarray = Array();
            T31Tool.moveFirst();
        }  
        else
            Ext.Msg.alert('訊息', '查詢條件的歸戶代碼為必填');
        msglabel('');
    }

    var cbFlag = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'INID_FLAG',
        fieldLabel: '單位類別註記',
        queryParam: {
            GRP_CODE: 'UR_INID',
            DATA_NAME: 'INID_FLAG'
        },
        //storeAutoLoad: true,
        //insertEmptyRow: true,
        editable: false,
        forceSelection: true,
        readOnly: true
    });

    var mmCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0041/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            return { INID: T1Form.getForm().findField('INID').getValue() };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                alert(r.get('MAT_CLASS'));
            }
        }
    });

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90,
            labelAlign: 'right'
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            name: 'INID_O',
            xtype: 'hidden'
        }, {
            fieldLabel: '責任中心代碼',
            name: 'INID',
            enforceMaxLength: true,
            maxLength: 6,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '責任中心名稱',
            name: 'INID_NAME',
            enforceMaxLength: true,
            maxLength: 30,
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '刪除註記',
            name: 'INID_OLD'
        },
            cbFlag,
            {
                xtype: 'displayfield',
                fieldLabel: '紀錄新增日期/時間',
                name: 'CREATE_TIME',
                renderer: function (value, meta, record) {
                    return Ext.util.Format.date(value, 'X/m/d H:i:s');
                }
            },
            {
                xtype: 'displayfield',
                fieldLabel: '紀錄新增人員',
                name: 'CREATE_USER_NAME'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '紀錄更新日期/時間',
                name: 'UPDATE_TIME',
                renderer: function (value, meta, record) {
                    return Ext.util.Format.date(value, 'X/m/d H:i:s');
                }
            },
            {
                xtype: 'displayfield',
                fieldLabel: '紀錄更新人員',
                name: 'UPDATE_USER_NAME'
            }
            /*, mmCode */
        ],

        buttons: [{
            itemId: 'T1submit', text: '儲存', hidden: true,
            handler: function () {
                var f = T1Form.getForm();
                if (f.isValid()) {
                    var confirmSubmit = viewport.down('#t1FormSub').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );
                }
                else {
                    Ext.Msg.alert('訊息', '輸入資料有誤');
                }
            }
        }, {
            itemId: 'T1cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });

    var T2Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90,
            labelAlign: 'right'
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            fieldLabel: '歸戶代碼',
            name: 'GRP_NO',
            enforceMaxLength: true,
            maxLength: 2,
            readOnly: true,
            regexText: '只能輸入英文字母與數字',
            regex: /^[\w-]{0,13}$/,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '歸戶名稱',
            name: 'GRP_NAME',
            enforceMaxLength: true,
            maxLength: 15,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }
        ],

        buttons: [{
            itemId: 'T2submit', text: '儲存', hidden: true,
            handler: function () {
                var f = T2Form.getForm();
                if (f.isValid()) {
                    var confirmSubmit = viewport.down('#t2FormSub').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T2Submit();
                        }
                    }
                    );
                }
                else {
                    Ext.Msg.alert('訊息', '輸入資料有誤');
                }
            }
        }, {
            itemId: 'T2cancel', text: '取消', hidden: true, handler: T2Cleanup
        }]
    });

    function T1Submit() {
        var f = T1Form.getForm();
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
                        T1Store.removeAll();
                        var v = action.result.etts[0];
                        r.set(v);
                        T1Store.insert(0, r);
                        r.commit();
                        T1Query.getForm().findField('P0').setValue(r.get('INID'));
                        msglabel('資料新增完成');
                        break;
                    case "U":
                        var v = action.result.etts[0];
                        r.set(v);
                        r.commit();
                        msglabel('資料修改完成');
                        break;
                    case "D":
                        var v = action.result.etts[0];
                        r.set(v);
                        r.commit();
                        msglabel('資料刪除完成');
                        break;
                    /*
                    T1Store.remove(r);
                    r.commit();
                    break;
                    */
                }
                T1Cleanup();
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

    function T2Submit() {
        var f = T2Form.getForm();
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        f.submit({
            url: T2Set,
            success: function (form, action) {
                myMask.hide();
                var f2 = T2Form.getForm();
                var r = f2.getRecord();
                switch (f2.findField("x").getValue()) {
                    case "I":
                        T2Store.removeAll();
                        var v = action.result.etts[0];
                        r.set(v);
                        T2Store.insert(0, r);
                        r.commit();
                        T2Query.getForm().findField('P0').setValue(r.get('GRP_NO'));
                        msglabel('資料新增完成');
                        break;
                    case "U":
                        var v = action.result.etts[0];
                        r.set(v);
                        r.commit();
                        msglabel('資料修改完成');
                        break;
                    case "D":
                        T2Store.remove(r);
                        r.commit();
                        msglabel('資料刪除完成');
                        break;
                    /*
                    T1Store.remove(r);
                    r.commit();
                    break;
                    */
                }
                T2Cleanup();
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

    function T1Cleanup() {
        viewport.down('#t1GridSub').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.findField('INID').setReadOnly(true);
        f.findField('INID_NAME').setReadOnly(true);
        f.findField('INID_FLAG').setReadOnly(true);
        T1Form.down('#T1cancel').hide();
        T1Form.down('#T1submit').hide();
        viewport.down('#t1FormSub').setTitle('瀏覽');
        setFormT1a();
    }

    function T2Cleanup() {
        viewport.down('#t2GridSub').unmask();
        var f = T2Form.getForm();
        f.reset();
        f.findField('GRP_NO').setReadOnly(true);
        f.findField('GRP_NAME').setReadOnly(true);
        T2Form.down('#T2cancel').hide();
        T2Form.down('#T2submit').hide();
        viewport.down('#t2FormSub').setTitle('瀏覽');
        setFormT2a();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'T1add', text: '新增', handler: function () {
                    T1Set = T1Create;
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'T1edit', text: '修改', disabled: true, handler: function () {
                    T1Set = T1Update;
                    setFormT1("U", '修改');
                }
            },
            {
                itemId: 'T1delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = T1Delete;
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            },
            {
                itemId: 'export', text: '匯出', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'FN', value: '責任中心基本檔匯出.xls' }); //檔名
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件
                            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() }); //SQL篩選條件
                            WEBAPP.utils.Common.postForm(T1Exp, p);
                        }
                    });
                }
            }
        ]
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'T2add', text: '新增', handler: function () {
                    T2Set = T2Create;
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'T2edit', text: '修改', disabled: true, handler: function () {
                    T2Set = T2Update;
                    setFormT2("U", '修改');
                }
            },
            {
                itemId: 'T2delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T2Set = T2Delete;
                            T2Form.getForm().findField('x').setValue('D');
                            T2Submit();
                        }
                    }
                    );
                }
            }
        ]
    });

    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '刪除', itemId: 'T3Dbtn', disabled: true, handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                actionMethods: {
                                    read: 'POST' // by default GET
                                },
                                async: false,     // 同步處理
                                url: T3Del,
                                params: {
                                    p0: T3GridShare.selectarray
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        Ext.Msg.alert('訊息', '刪除成功!');
                                        msglabel('資料刪除成功');
                                        T3GridShare.selectarray = Array();                   //清空 MAster Grid 勾選筆數
                                        T3Store.load();
                                        T31Load();
                                    }
                                },
                                failure: function (response, options) {
                                    Ext.Msg.alert('訊息', 'failure!');
                                }
                            });
                        }
                    });
                }
            }
        ]
    });

    var T31Tool = Ext.create('Ext.PagingToolbar', {
        store: T31Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '指定', itemId: 'T31Abtn', disabled:true, handler: function () {
                    Ext.MessageBox.confirm('指定', '是否將選擇的責任中心進行指定？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                actionMethods: {
                                    read: 'POST' // by default GET
                                },
                                async: false,     // 同步處理
                                url: T31Add,
                                params: {
                                    p0: T31GridShare.selectarray
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        Ext.Msg.alert('訊息', '指定成功!');
                                        msglabel('資料指定成功');
                                        T31GridShare.selectarray = Array();                   //清空 MAster Grid 勾選筆數
                                        T3Store.load();
                                        T31Load();
                                    }
                                    else
                                        Ext.Msg.alert('訊息', '選擇的責任中心已被指定,請重新查詢!');
                                },
                                failure: function (response, options) {
                                    Ext.Msg.alert('訊息', 'failure!');
                                }
                            });
                        }
                    });
                }
            }
        ]
    });

    function setFormT1(x, t) {
        viewport.down('#t1GridSub').mask();
        viewport.down('#t1FormSub').setTitle(t + T1Name);
        viewport.down('#t1FormSub').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            f.reset();
            var r = Ext.create('WEBAPP.model.UrInid');
            T1Form.loadRecord(r);
            u = f.findField("INID");
            u.setReadOnly(false);
        }
        else {
            u = f.findField('INID_NAME');
        }
        f.findField('x').setValue(x);
        f.findField('INID').setReadOnly(false);
        f.findField('INID_NAME').setReadOnly(false);
        f.findField('INID_FLAG').setReadOnly(false);
        T1Form.down('#T1cancel').setVisible(true);
        T1Form.down('#T1submit').setVisible(true);
        u.focus();
    }

    function setFormT2(x, t) {
        viewport.down('#t2GridSub').mask();
        viewport.down('#t2FormSub').setTitle(t + T1Name);
        viewport.down('#t2FormSub').expand();
        var f = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            f.reset();
            var r = Ext.create('WEBAPP.model.UrInidGrp');
            T2Form.loadRecord(r);
            u = f.findField("GRP_NO");
            u.setReadOnly(false);
        }
        else {
            u = f.findField('GRP_NAME');
        }
        f.findField('x').setValue(x);
        f.findField('GRP_NAME').setReadOnly(false);
        T2Form.down('#T2cancel').setVisible(true);
        T2Form.down('#T2submit').setVisible(true);
        u.focus();
    }

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
        }
        ],
        
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "責任中心代碼",
            dataIndex: 'INID',
            width: 100
        }, {
            text: "責任中心名稱",
            dataIndex: 'INID_NAME',
            width: 100
        }, {
            text: "刪除註記",
            dataIndex: 'INID_OLD',
            width: 80
        }, {
            text: "單位類別註記",
            dataIndex: 'INID_FLAG_NAME',
            width: 100
        }, {
            text: "歸戶",
            dataIndex: 'GRP_NO',
            width: 120
        }, {
            xtype: 'datecolumn',
            text: "紀錄更新日期/時間",
            dataIndex: 'UPDATE_TIME',
            format: 'Xmd/His', //X=民國年,m=月,d=日,H=時,i=分,s=秒
            width: 120
        }, {
            text: "紀錄更新人員",
            dataIndex: 'UPDATE_USER_NAME',
            width: 100
        }, {
            sortable: false,
            flex: 1
        }],
        listeners: {
            //render: function (view) {
            //    view.setLoading('Loading Grid...');
            //},
            //viewready: function (view) {
            //    view.setLoading({
            //        store: view.getStore()
            //    }).hide();
            //},
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
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
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "歸戶代碼",
            dataIndex: 'GRP_NO',
            width: 80
        }, {
            text: "歸戶名稱",
            dataIndex: 'GRP_NAME',
            width: 170
        }, {
            sortable: false,
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
            }
        }
    });

    Ext.define('T3GridShare', {
        selectarray: Array(),
        insertarray: function (key) {
            var checkexist = false;
            Ext.Array.each(T3GridShare.selectarray, function (id) {
                if (id == key) {
                    checkexist = true;
                    return;
                }
            });
            if (!checkexist) {
                T3GridShare.selectarray.push(key);
            }
        },
        deletearray: function (key) {
            var intPos = Ext.Array.indexOf(T3GridShare.selectarray, key, 0);   // 找出checkbox deselect在selectarray陣列中是第幾個
            T3GridShare.selectarray.splice(intPos, 1)                          // 將 checkbox deselect 的選項在 selectarray陣列中移除                
        },
        singleton: true
    });
    var checkboxT3Model = Ext.create('Ext.selection.CheckboxModel', {
        listeners: {
            'select': function (view, rec) {
                //把勾選的資料存到array
                T3GridShare.insertarray(rec.get('GRP_NO') + "^" + rec.get('INID') );
            },
            'deselect': function (view, rec) {
                //把勾選的資料從array移除
                T3GridShare.deletearray(rec.get('GRP_NO') + "^" + rec.get('INID'));
            }
        }
    });
    var T3Grid = Ext.create('Ext.grid.Panel', {
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T3Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T3Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "歸戶代碼",
            dataIndex: 'GRP_NO',
            width: 80
        }, {
            text: "責任中心代碼",
            dataIndex: 'INID',
            width: 100
        }, {
            text: "責任中心名稱",
            dataIndex: 'INID_NAME',
            width: 170
        }, {
            sortable: false,
            flex: 1
        }],
        selModel: checkboxT3Model,
        listeners: {
            selectionchange: function (model, records) {
                T3Rec = records.length;
                T3LastRec = records[0];
                T3Tool.down('#T3Dbtn').setDisabled(T3Rec === 0);
            }
        }
    });

    Ext.define('T31GridShare', {
        selectarray: Array(),
        insertarray: function (key) {
            var checkexist = false;
            Ext.Array.each(T31GridShare.selectarray, function (id) {
                if (id == key) {
                    checkexist = true;
                    return;
                }
            });
            if (!checkexist) {
                T31GridShare.selectarray.push(key);
            }
        },
        deletearray: function (key) {
            var intPos = Ext.Array.indexOf(T31GridShare.selectarray, key, 0);   // 找出checkbox deselect在selectarray陣列中是第幾個
            T31GridShare.selectarray.splice(intPos, 1)                          // 將 checkbox deselect 的選項在 selectarray陣列中移除                
        },
        singleton: true
    });
    var checkboxT31Model = Ext.create('Ext.selection.CheckboxModel', {
        listeners: {
            'select': function (view, rec) {
                //把勾選的資料存到array
                T31GridShare.insertarray(T3Q_GRP_NO + "^" + rec.get('INID'));
            },
            'deselect': function (view, rec) {
                //把勾選的資料從array移除
                T31GridShare.deletearray(T3Q_GRP_NO + "^" + rec.get('INID'));
            }
        }
    });
    var T31Grid = Ext.create('Ext.grid.Panel', {
        store: T31Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T31Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T31Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "責任中心代碼",
            dataIndex: 'INID',
            width: 100
        }, {
            text: "責任中心名稱",
            dataIndex: 'INID_NAME',
            width: 170
        }, {
            sortable: false,
            flex: 1
        }],
        selModel: checkboxT31Model,
        listeners: {
            selectionchange: function (model, records) {
                T31Rec = records.length;
                T31LastRec = records[0];
                T31Tool.down('#T31Abtn').setDisabled(T31Rec === 0);
            }
        }
    });

    function setFormT1a() {
        T1Grid.down('#T1edit').setDisabled(T1Rec === 0);
        T1Grid.down('#T1delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var inid_old = T1LastRec.get('INID_OLD');
            if (Ext.String.trim(inid_old) == 'D') {
                T1Grid.down('#T1edit').setDisabled(true);
                T1Grid.down('#T1delete').setDisabled(true);
            }
            else {
                var f = T1Form.getForm();
                f.findField('x').setValue('U');
                var u = f.findField('INID');
                u.setReadOnly(true);
                u.setFieldStyle('border: 0px');
                f.findField('INID_O').setValue(T1LastRec.get('INID'));
            }
        }
        else {
            T1Form.getForm().reset();
        }
    }

    function setFormT2a() {
        T2Grid.down('#T2edit').setDisabled(T2Rec === 0);
        T2Grid.down('#T2delete').setDisabled(T2Rec === 0);
        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);

            var f = T2Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('GRP_NO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
        }
        else {
            T2Form.getForm().reset();
        }
    }

    var Tabs = Ext.widget('tabpanel', {
        itemId: 'TabPanel',
        region: 'center',
        layout: 'fit',
        collapsible: false,
        title: '',
        border: false,
        defaults: {
            layout: 'fit',
            split: true
        },
        items: [{
            itemId: 't1Grid',
            title: '責任中心基本檔',
            layout: 'hbox',
            items: [{
                itemId: 't1GridSub',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                width: '75%',
                height: '100%',
                items: [T1Grid]
            },
            {
                itemId: 't1FormSub',
                region: 'east',
                // collapsible: true,
                floatable: true,
                width: '25%',
                height: '100%',
                title: '瀏覽',
                split: true,
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form]
            }]
        }, {
            itemId: 't2Grid',
            title: '歸戶基本檔',
            layout: 'hbox',
            hidden: true,
            items: [{
                itemId: 't2GridSub',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                width: '75%',
                height: '100%',
                items: [T2Grid]
            },
            {
                itemId: 't2FormSub',
                region: 'east',
                // collapsible: true,
                floatable: true,
                width: '25%',
                height: '100%',
                title: '瀏覽',
                split: true,
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T2Form]
            }]
            }, {
            itemId: 't3Grid',
            title: '歸戶責任中心對照檔',
            layout: 'border',
            hidden: true,
            items: [{
                itemId: 't3GridSub',
                region: 'north',
                layout: 'fit',
                collapsible: false,
                title: '',
                width: '100%',
                height: '50%',
                split: true,
                items: [T3Grid]
            }, {
                itemId: 't31GridSub',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                width: '100%',
                height: '50%',
                items: [T31Grid]
            }]
        }],
        listeners: {
            tabchange: function (panel, newTab, oldTab, eOpts) {
                if (newTab.itemId == 't3Grid') {
                    // 重新載入歸戶代碼選項
                    setGrpComboData();
                }
                
            }
        }
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
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            split: true,
            height: '100%',
            items: [Tabs]
        }]
    });

    // 由其他程式呼叫時,若有傳入參數則依參數控制三個tab內的按鈕
    // 例如111, 第一碼控制第一個tab,第二碼控制第二個tab...
    function setEditBtnVisible() {
        if (parBtnVis)
        {
            if (parBtnVis.substring(0, 1) == '0')
            {
                T1Grid.down('#T1add').setVisible(false);
                T1Grid.down('#T1edit').setVisible(false); 
                T1Grid.down('#T1delete').setVisible(false);
                T1Grid.down('#export').setVisible(false);
            }
            if (parBtnVis.substring(1, 2) == '0') {
                T2Grid.down('#T2add').setVisible(false);
                T2Grid.down('#T2edit').setVisible(false);
                T2Grid.down('#T2delete').setVisible(false);
            }
            if (parBtnVis.substring(2, 3) == '0') {
                T3Grid.down('#T3Dbtn').setVisible(false);
                T31Grid.down('#T31Abtn').setVisible(false);
            }
        }
    }
    setEditBtnVisible();

    function getHospCode() {
        Ext.Ajax.request({
            url: '/api/FA0067/GetHospCode', //與FA0067共用取得醫院代碼
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == '0') {
                        viewport.down('#TabPanel').getTabBar().items.get(1).show();
                        viewport.down('#TabPanel').getTabBar().items.get(2).show();
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getHospCode();

    T1Query.getForm().findField('P0').focus();
});
