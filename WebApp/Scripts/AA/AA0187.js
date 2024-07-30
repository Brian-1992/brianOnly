Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "各單位盤點狀況查詢";
    var T1GetExcel = '/api/AA0187/Excel';
    var reportUrl = '/Report/A/AA0187.aspx';
    var T1RecLength = 0;
    var T1LastRec = null;
    var p0_default = "";

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var isAA = Ext.getUrlParam('isAA');
    var menuLink = Ext.getUrlParam('menuLink');

    //庫房代碼
    var SetWhnoComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetWhnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    isAA: isAA=='Y' ? isAA : "N"
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {
                var DataCount = store.getCount();
                if (DataCount > 0 && isAA) {
                    var temp = [];
                    //temp.push(store.getAt(0).get('VALUE'));
                    //temp.push(store.getAt(1).get('VALUE'));
                    //T1QueryForm.getForm().findField('WH_NO').setValue(temp);
                }
            }
        },
        autoLoad: true
    });
    //預設庫房代碼
    function SetDefaultWhNo() {
        Ext.Ajax.request({
            url: '/api/AA0187/GetDefaultWhNo',
            method: reqVal_p,
            params: {
                menuCode: menuLink
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (menuLink == "AA0212" || menuLink == "FA0083") {
                        p0_default = data.etts[0];
                        T1QueryForm.getForm().findField('WH_NO').setValue(p0_default);
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    if (menuLink == "AA0212" || menuLink == "FA0083") {
        SetDefaultWhNo();
    }
    //藥材類別
    var SetMatClassSubParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetMatClassSubParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P5 = T1QueryForm.getForm().findField('MAT_CLASS');
                if (DataCount > 0) {
                    combo_P5.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //是否合約
    var SetContidParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetContidParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P7 = T1QueryForm.getForm().findField('M_CONTID');
                if (DataCount > 0) {
                    combo_P7.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //買斷寄庫
    var SetSourcecodeParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetSourcecodeParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P8 = T1QueryForm.getForm().findField('E_SOURCECODE');
                if (DataCount > 0) {
                    combo_P8.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //是否戰備
    var SetWarbakParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetWarbakParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P9 = T1QueryForm.getForm().findField('WARBAK');
                if (DataCount > 0) {
                    combo_P9.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //管制品項
    var SetRestrictcodeParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetRestrictcodeParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P10 = T1QueryForm.getForm().findField('E_RESTRICTCODE');
                if (DataCount > 0) {
                    combo_P10.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //是否常用品項
    var SetCommonParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetCommonParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P11 = T1QueryForm.getForm().findField('COMMON');
                if (DataCount > 0) {
                    combo_P11.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //急救品項
    var SetFastdrugParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetFastdrugParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P12 = T1QueryForm.getForm().findField('FASTDRUG');
                if (DataCount > 0) {
                    combo_P12.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //中西藥別
    var SetDrugkindParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetDrugkindParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P13 = T1QueryForm.getForm().findField('DRUGKIND');
                if (DataCount > 0) {
                    combo_P13.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //合約類別
    var SetTouchcaseParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetTouchcaseParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P14 = T1QueryForm.getForm().findField('TOUCHCASE');
                if (DataCount > 0) {
                    combo_P14.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //採購類別
    var SetOrderkindParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetOrderkindParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P15 = T1QueryForm.getForm().findField('ORDERKIND');
                if (DataCount > 0) {
                    combo_P15.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //特殊品項
    var SetSpecialorderkindParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetSpecialorderkindParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P16 = T1QueryForm.getForm().findField('SPDRUG');
                if (DataCount > 0) {
                    combo_P16.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //是否特材
    var SetSpxfeeParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetSpxfeeParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P16 = T1QueryForm.getForm().findField('SPXFEE');
                if (DataCount > 0) {
                    combo_P16.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //廠商代碼
    var SetAgennoComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetAgennoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P16 = T1QueryForm.getForm().findField('AGEN_NO');
                if (DataCount > 0) {
                    combo_P16.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //是否點滴
    var SetIsivParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetIsivParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P16 = T1QueryForm.getForm().findField('ISIV');
                if (DataCount > 0) {
                    combo_P16.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });

    var mLabelWidth = 70;
    var mWidth = 230;
    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '藥材代碼',
        labelAlign: 'right',
        limit: 50, //限制一次最多顯示10筆
        queryUrl: '/api/AA0187/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    //右方查詢欄位
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
                store: SetWhnoComboGet,
                fieldLabel: '庫房代碼',
                name: 'WH_NO',
                id: 'WH_NO',
                queryMode: 'local',
                labelAlign: 'right',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '藥材類別',
                name: 'MAT_CLASS',
                id: 'MAT_CLASS',
                store: SetMatClassSubParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            T1QueryMMCode,
            {
                xtype: 'combo',
                fieldLabel: '廠商代碼',
                name: 'AGEN_NO',
                id: 'AGEN_NO',
                store: SetAgennoComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'textfield',
                fieldLabel: '廠商名稱',
                name: 'AGEN_NAME',
                id: 'AGEN_NAME',
            },
            {
                xtype: 'combo',
                fieldLabel: '是否合約',
                name: 'M_CONTID',
                id: 'M_CONTID',
                store: SetContidParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '買斷寄庫',
                name: 'E_SOURCECODE',
                id: 'E_SOURCECODE',
                store: SetSourcecodeParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '是否戰備',
                name: 'WARBAK',
                id: 'WARBAK',
                store: SetWarbakParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '管制品項',
                name: 'E_RESTRICTCODE',
                id: 'E_RESTRICTCODE',
                store: SetRestrictcodeParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '是否常用品項',
                name: 'COMMON',
                id: 'COMMON',
                store: SetCommonParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '急救品項',
                name: 'FASTDRUG',
                id: 'FASTDRUG',
                store: SetFastdrugParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '中西藥別',
                name: 'DRUGKIND',
                id: 'DRUGKIND',
                store: SetDrugkindParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '合約類別',
                name: 'TOUCHCASE',
                id: 'TOUCHCASE',
                store: SetTouchcaseParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '採購類別',
                name: 'ORDERKIND',
                id: 'ORDERKIND',
                store: SetOrderkindParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '特殊品項',
                name: 'SPDRUG',
                id: 'SPDRUG',
                store: SetSpecialorderkindParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '是否特材',
                name: 'SPXFEE',
                id: 'SPXFEE',
                store: SetSpxfeeParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '是否點滴',
                name: 'ISIV',
                id: 'ISIV',
                store: SetIsivParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'checkbox',
                name: 'QTYISNOTZERO',
                id: 'QTYISNOTZERO',
                boxLabel: '存量<>0',
                checked: false
            },
            {
                xtype: 'checkbox',
                name: 'QTYISZERO',
                id: 'QTYISZERO',
                boxLabel: '存量=0',
                checked: false
            },
            {
                xtype: 'checkbox',
                name: 'QTYISCHANGED',
                id: 'QTYISCHANGED',
                boxLabel: '庫存有異動',
                checked: false
            },
            {
                xtype: 'checkbox',
                name: 'QTYISNOTCHANGED',
                id: 'QTYISNOTCHANGED',
                boxLabel: '庫存無異動',
                checked: false
            },
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
                T1QueryForm.getForm().findField('WH_NO').setValue('');
                T1QueryForm.getForm().findField('MMCODE').setValue('');
                T1QueryForm.getForm().findField('WARBAK').setValue(' ');
                T1QueryForm.getForm().findField('MAT_CLASS').setValue(' ');
                T1QueryForm.getForm().findField('M_CONTID').setValue(' ');
                T1QueryForm.getForm().findField('E_SOURCECODE').setValue(' ');
                T1QueryForm.getForm().findField('E_RESTRICTCODE').setValue(' ');
                T1QueryForm.getForm().findField('COMMON').setValue(' ');
                T1QueryForm.getForm().findField('SPDRUG').setValue(' ');
                T1QueryForm.getForm().findField('FASTDRUG').setValue(' ');
                T1QueryForm.getForm().findField('ISIV').setValue(' ');
                T1QueryForm.getForm().findField('ORDERKIND').setValue(' ');
                T1QueryForm.getForm().findField('SPXFEE').setValue(' ');
                T1QueryForm.getForm().findField('DRUGKIND').setValue(' ');
                T1QueryForm.getForm().findField('TOUCHCASE').setValue(' ');
                T1QueryForm.getForm().findField('AGEN_NO').setValue(' ');
                T1QueryForm.getForm().findField('AGEN_NAME').setValue('');
                T1QueryForm.getForm().findField('QTYISZERO').setValue('');
                T1QueryForm.getForm().findField('QTYISNOTZERO').setValue('');
                T1QueryForm.getForm().findField('QTYISCHANGED').setValue('');
                T1QueryForm.getForm().findField('QTYISNOTCHANGED').setValue('');
                msglabel('訊息區:');
            }
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 100, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'WH_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetAll',
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
                    isAA: isAA=='Y' ? "Y" : "N",
                    WH_NO: T1QueryForm.getForm().findField('WH_NO').getValue(),
                    MMCODE: T1QueryForm.getForm().findField('MMCODE').getValue(),
                    WARBAK: T1QueryForm.getForm().findField('WARBAK').getValue(),
                    MAT_CLASS: T1QueryForm.getForm().findField('MAT_CLASS').getValue(),
                    M_CONTID: T1QueryForm.getForm().findField('M_CONTID').getValue(),
                    E_SOURCECODE: T1QueryForm.getForm().findField('E_SOURCECODE').getValue(),
                    E_RESTRICTCODE: T1QueryForm.getForm().findField('E_RESTRICTCODE').getValue(),
                    COMMON: T1QueryForm.getForm().findField('COMMON').getValue(),
                    SPDRUG: T1QueryForm.getForm().findField('SPDRUG').getValue(),
                    FASTDRUG: T1QueryForm.getForm().findField('FASTDRUG').getValue(),
                    ISIV: T1QueryForm.getForm().findField('ISIV').getValue(),
                    ORDERKIND: T1QueryForm.getForm().findField('ORDERKIND').getValue(),
                    SPXFEE: T1QueryForm.getForm().findField('SPXFEE').getValue(),
                    DRUGKIND: T1QueryForm.getForm().findField('DRUGKIND').getValue(),
                    TOUCHCASE: T1QueryForm.getForm().findField('TOUCHCASE').getValue(),
                    AGEN_NO: T1QueryForm.getForm().findField('AGEN_NO').getValue(),
                    AGEN_NAME: T1QueryForm.getForm().findField('AGEN_NAME').getValue(),
                    QTYISZERO: T1QueryForm.getForm().findField('QTYISZERO').getValue(),
                    QTYISNOTZERO: T1QueryForm.getForm().findField('QTYISNOTZERO').getValue(),
                    QTYISCHANGED: T1QueryForm.getForm().findField('QTYISCHANGED').getValue(),
                    QTYISNOTCHANGED: T1QueryForm.getForm().findField('QTYISNOTCHANGED').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T1Tool.down('#export').setDisabled(false);
                    }
                    else {
                        T1Tool.down('#export').setDisabled(true);
                        msglabel('查無資料!');
                        Ext.Msg.alert('提醒', '查無資料!');
                    }
                }
            }
        },
    });

    //表單工具列
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'ISAA', value: isAA=='Y' ? "Y" : "N" }); //SQL篩選條件 
                            p.push({ name: 'WH_NO', value: T1QueryForm.getForm().findField('WH_NO').getValue() }); //SQL篩選條件 
                            p.push({ name: 'MMCODE', value: T1QueryForm.getForm().findField('MMCODE').getValue() }); //SQL篩選條件 
                            p.push({ name: 'WARBAK', value: T1QueryForm.getForm().findField('WARBAK').getValue() }); //SQL篩選條件 
                            p.push({ name: 'MAT_CLASS', value: T1QueryForm.getForm().findField('MAT_CLASS').getValue() }); //SQL篩選條件 
                            p.push({ name: 'M_CONTID', value: T1QueryForm.getForm().findField('M_CONTID').getValue() }); //SQL篩選條件 
                            p.push({ name: 'E_SOURCECODE', value: T1QueryForm.getForm().findField('E_SOURCECODE').getValue() }); //SQL篩選條件 
                            p.push({ name: 'E_RESTRICTCODE', value: T1QueryForm.getForm().findField('E_RESTRICTCODE').getValue() }); //SQL篩選條件 
                            p.push({ name: 'COMMON', value: T1QueryForm.getForm().findField('COMMON').getValue() }); //SQL篩選條件 
                            p.push({ name: 'SPDRUG', value: T1QueryForm.getForm().findField('SPDRUG').getValue() }); //SQL篩選條件 
                            p.push({ name: 'FASTDRUG', value: T1QueryForm.getForm().findField('FASTDRUG').getValue() }); //SQL篩選條件 
                            p.push({ name: 'ISIV', value: T1QueryForm.getForm().findField('ISIV').getValue() }); //SQL篩選條件 
                            p.push({ name: 'ORDERKIND', value: T1QueryForm.getForm().findField('ORDERKIND').getValue() }); //SQL篩選條件
                            p.push({ name: 'SPXFEE', value: T1QueryForm.getForm().findField('SPXFEE').getValue() }); //SQL篩選條件
                            p.push({ name: 'DRUGKIND', value: T1QueryForm.getForm().findField('DRUGKIND').getValue() }); //SQL篩選條件
                            p.push({ name: 'TOUCHCASE', value: T1QueryForm.getForm().findField('TOUCHCASE').getValue() }); //SQL篩選條件
                            p.push({ name: 'AGEN_NO', value: T1QueryForm.getForm().findField('AGEN_NO').getValue() }); //SQL篩選條件
                            p.push({ name: 'AGEN_NAME', value: T1QueryForm.getForm().findField('AGEN_NAME').getValue() }); //SQL篩選條件
                            p.push({ name: 'QTYISZERO', value: T1QueryForm.getForm().findField('QTYISZERO').getValue() }); //SQL篩選條件
                            p.push({ name: 'QTYISNOTZERO', value: T1QueryForm.getForm().findField('QTYISNOTZERO').getValue() }); //SQL篩選條件
                            p.push({ name: 'QTYISCHANGED', value: T1QueryForm.getForm().findField('QTYISCHANGED').getValue() }); //SQL篩選條件
                            p.push({ name: 'QTYISNOTCHANGED', value: T1QueryForm.getForm().findField('QTYISNOTCHANGED').getValue() }); //SQL篩選條件
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            },
            {
                text: '列印',
                id: 'btnPrint',
                name: 'btnPrint',
                hidden: ((isAA != 'Y' && menuLink != "FA0083") || menuLink == "FA0083"),
                handler: function () {
                    //T1Form.getForm().findField('print_order').setValue('store_loc, mmcode, chk_uid');
                    printWindow.show();
                }
            },
        ]
    });

    //藥材種類
    var SetMatClassComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0187/GetMatClassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    isAA: isAA ? "Y" : "N",
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {
            }
        },
        autoLoad: true
    });

    var T1QueryAgenNoCombo = Ext.create('WEBAPP.form.AgenNoCombo_2', {
        name: 'P2',
        id: 'P2',
        fieldLabel: '選擇廠商區間',
        labelAlign: 'right',
        //fieldCls: 'required',
        //allowBlank: false,
        labelWidth: 100,
        //width: 290,
        limit: 50,//限制一次最多顯示50筆
        queryUrl: '/api/BG0011/GetAgenNoCombo',//指定查詢的Controller路徑
        extraFields: ['AGEN_NO', 'AGEN_NAMEC'],//查詢完會回傳的欄位
        getDefaultParams: function () {//查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
            }
        }
    });

    var T1QueryAgenNoCombo_1 = Ext.create('WEBAPP.form.AgenNoCombo_2', {
        name: 'P3',
        id: 'P3',
        fieldLabel: '至',
        labelAlign: 'right',
        //fieldCls: 'required',
        //allowBlank: false,
        labelWidth: 30,
        //width: 150,
        limit: 50,//限制一次最多顯示50筆
        queryUrl: '/api/BG0011/GetAgenNoCombo_1',//指定查詢的Controller路徑
        extraFields: ['AGEN_NO', 'AGEN_NAMEC'],//查詢完會回傳的欄位
        getDefaultParams: function () {//查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
            }
        }
    });
    // 排序方式清單
    var printOrderStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "mmname", "TEXT": "0. 藥材名稱" },
            { "VALUE": "mmcode", "TEXT": "1. 藥材代碼" },
            { "VALUE": "store_loc", "TEXT": "2. 儲位" },
            { "VALUE": "m_agenno", "TEXT": "3. 廠商代碼" },
        ]
    });
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            padding: '0 4 0 4',
            items: [
                {
                    xtype: 'container',
                    border: false,
                    layout: 'vbox',
                    items: [
                        {
                            xtype: 'datefield',
                            fieldLabel: '列印日期',
                            name: 'P0',
                            id: 'P0',
                            labelWidth: 90,
                            value: getTodayDate(),
                        },
                        {
                            xtype: 'fieldset',
                            title: '列印藥材資料',
                            layout: 'vbox',
                            items: [
                                {
                                    xtype: 'combo',
                                    fieldLabel: '選擇藥材類別',
                                    name: 'P1',
                                    id: 'P1',
                                    store: SetMatClassComboGet,
                                    labelWidth: 100,
                                    queryMode: 'local',
                                    displayField: 'TEXT',
                                    valueField: 'VALUE',
                                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
                                },
                                {
                                    xtype: 'panel',
                                    id: 'PanelP1',
                                    border: false,
                                    layout: 'hbox',
                                    items: [T1QueryAgenNoCombo, T1QueryAgenNoCombo_1,]
                                },
                                {
                                    xtype: 'combo',
                                    store: printOrderStore,
                                    name: 'P4',
                                    id: 'P4',
                                    fieldLabel: '排序方式',
                                    displayField: 'TEXT',
                                    valueField: 'VALUE',
                                    queryMode: 'local',
                                    anyMatch: true,
                                    allowBlank: false,
                                    typeAhead: true,
                                    forceSelection: true,
                                    triggerAction: 'all',
                                    multiSelect: false,
                                    fieldCls: 'required',
                                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                                    labelWidth: 100,
                                    margin: '5 0 0 0',
                                },
                                {
                                    xtype: 'panel',
                                    id: 'PanelP2',
                                    border: false,
                                    layout: 'hbox',
                                    items: [
                                        {
                                            xtype: 'radiogroup',
                                            name: 'P5',
                                            id: 'P5',
                                            width: 280,
                                            items: [
                                                { boxLabel: '由小而大排序', name: 'option', inputValue: 'ASC', padding: '0 5 0 40', checked: true },
                                                { boxLabel: '由大而小排序', name: 'option', inputValue: 'DESC', },
                                            ]
                                        },
                                        {
                                            xtype: 'checkbox',
                                            name: 'P6',
                                            id: 'P6',
                                            boxLabel: '隱藏庫存量為0',
                                            checked: true
                                        },
                                    ]
                                },
                            ]
                        },
                    ]
                },
            ]
        }]
    });

    var printWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T1Form],
        resizable: false,
        draggable: false,
        closable: false,
        title: "輸入列印日期及其他資料",
        buttons: [
            {
                text: '確定',
                handler: function () {
                    if (!T1Form.getForm().findField('P4').getValue()) {
                        Ext.Msg.alert('提醒', '請選擇列印排序方式');
                        return;
                    }
                    showReport(reportUrl);
                    printWindow.hide();
                }
            },
            {
                text: '取消',
                handler: function () {
                    printWindow.hide();
                }
            }
        ]
    });
    printWindow.hide();

    function showReport(reportUrl) {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                + '?P0=' + T1Form.getForm().findField('P0').rawValue // 列印日期
                + '&P1=' + T1Form.getForm().findField('P1').getValue() // 藥材種類
                + '&P2=' + T1Form.getForm().findField('P2').rawValue // 廠商區間起
                + '&P3=' + T1Form.getForm().findField('P3').rawValue // 廠商區間訖
                + '&P4=' + T1Form.getForm().findField('P4').getValue() // 排序方式
                + '&P5=' + T1Form.getForm().findField('P5').getValue().option // 排序方向
                + '&P6=' + T1Form.getForm().findField('P6').rawValue // 隱藏庫存量為0
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
        }
        win.show();
    }

    function getTodayDate() {
        // 取得現在的日期
        const currentDate = new Date();

        // 取得年、月、日
        const year = currentDate.getFullYear() - 1911; // 轉換為民國年
        const month = currentDate.getMonth() + 1; // 月份從 0 開始
        const day = currentDate.getDate();

        // 將年、月、日格式化為兩位數
        const formattedYear = year.toString().padStart(3, '0');
        const formattedMonth = month.toString().padStart(2, '0');
        const formattedDay = day.toString().padStart(2, '0');

        // 組合成民國年時間
        const taiwaneseDate = formattedYear + formattedMonth + formattedDay;

        return taiwaneseDate;
    }

    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
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
        }
        ],
        columns: [
            { xtype: 'rownumberer' },
            { text: "庫房代碼", dataIndex: "WH_NO", width: 85 },
            { text: "藥材代碼", dataIndex: "MMCODE", width: 85 },
            { text: "藥材名稱", dataIndex: "MMNAME", width: 150 },
            { text: "廠商代碼", dataIndex: "AGEN_NO", width: 85 },
            { text: "存量", dataIndex: "INV_QTY", width: 85 },
            { text: "上月結", dataIndex: "P_INVQTY", width: 85 },
            { text: "進貨/撥發入", dataIndex: "APL_INQTY", width: 85 },
            { text: "撥發出", dataIndex: "APL_OUTQTY", width: 85 },
            { text: "調撥入", dataIndex: "TRN_INQTY", width: 85 },
            { text: "調撥出", dataIndex: "TRN_OUTQTY", width: 85 },
            { text: "調帳入", dataIndex: "ADJ_INQTY", width: 85 },
            { text: "調帳出", dataIndex: "ADJ_OUTQTY", width: 85 },
            { text: "退料入", dataIndex: "BAK_INQTY", width: 85 },
            { text: "退料出", dataIndex: "BAK_OUTQTY", width: 85 },
            { text: "退貨量", dataIndex: "REJ_OUTQTY", width: 85 },
            { text: "報廢量", dataIndex: "DIS_OUTQTY", width: 85 },
            { text: "換貨入", dataIndex: "EXG_INQTY", width: 85 },
            { text: "換貨出", dataIndex: "EXG_OUTQTY", width: 85 },
            { text: "盤點差異量", dataIndex: "INVENTORYQTY", width: 85 },
            { text: "消耗量", dataIndex: "USE_QTY", width: 85 },
            { text: "出貨量", dataIndex: "UNITRATE", width: 85 },
            { text: "轉換量比", dataIndex: "TRUTRATE", width: 85 },
            { text: "庫存單價", dataIndex: "DISC_CPRICE", width: 85 },
            { text: "單位", dataIndex: "BASE_UNIT", width: 70 },
            { text: "廠商簡稱", dataIndex: "EASYNAME", width: 80 },
            { text: "管制品", dataIndex: "E_RESTRICTCODE", width: 80 },
            { text: "買斷寄庫", dataIndex: "E_SOURCECODE", width: 80 },
            { text: "案號", dataIndex: "CASENO", width: 120 },
            { text: "庫存成本", dataIndex: "AMOUNT", width: 80 },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                //T1Rec = records.length;
                //T1LastRec = records[0];
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
            itemId: 't1Grid',
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
                    height: '100%',
                    items: [T1Grid]
                }]
        }, {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '查詢',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1QueryForm]
        }]
    });

});