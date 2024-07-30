Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    // 國軍才秀金額相關欄位
    var showMoney = function () {
        return Ext.getUrlParam('isArmy') === "Y";
    };
    var menuLink = Ext.getUrlParam('menuLink');

    var T1Name = "各單位盤點狀況查詢";
    var T1GetExcel = '/api/CE0044/Excel';
    var T1GetCsv = '/api/CE0044/CSV';
    var T1GetExcel_805 = '/api/CE0044/Excel_805'; // [匯出消耗結存表] 為花蓮專用
    var T1GetExcel_803Detail = '/api/CE0044/ExcelDetail_803'; // [匯出消耗結存表] 為花蓮專用
    var reportUrl = '/Report/C/CE0044.aspx';

    var T1RecLength = 0;
    var T1LastRec = null;
    var p0_default = "";
    var p0_default2 = "";
    var p0_default3 = "";
    var hosp_code = '';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //月結年月
    var SetYmComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0044/GetYmCombo',
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
                    combo_P0.setValue(store.getAt(0).get('SET_YM'));
                    T1Query.getForm().findField('SET_BTIME').setValue(store.getAt(0).get('SET_BTIME'));
                    T1Query.getForm().findField('SET_CTIME').setValue(store.getAt(0).get('SET_CTIME'));
                    p0_default = store.getAt(0).get('SET_YM');
                    p0_default2 = store.getAt(0).get('SET_BTIME');
                    p0_default3 = store.getAt(0).get('SET_CTIME');
                }
            }
        },
        autoLoad: true
    });
    //盤存單位
    var SetRlnoComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0044/GetRlnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: menuLink
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P1 = T1Query.getForm().findField('P1');
                if (DataCount > 0) {
                    combo_P1.setValue(store.getAt(0).get('VALUE'));

                    if (store.getAt(0).get('VALUE') == '1') {
                        T1Query.getForm().findField('P3').setDisabled(false);
                        T1Query.getForm().findField('P4').setDisabled(false);
                        SetMiWhidComboGet(T1Query.getForm().findField('P2').getValue());
                    }
                    else {
                        T1Query.getForm().findField('P3').setDisabled(true);
                        T1Query.getForm().findField('P4').setDisabled(true);
                        T1Query.getForm().findField('P3').setValue("");
                        T1Query.getForm().findField('P4').setValue("");
                    }

                }
            }
        },
        autoLoad: true
    });
    //庫房類別
    var SetWhKindComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0044/GetWhKindCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P2 = T1Query.getForm().findField('P2');
                if (DataCount > 0) {
                    if (store.find('VALUE', '1') > 0)
                        combo_P2.setValue('1'); // 選擇各請領單位時預設查衛材
                    else
                        combo_P2.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //庫房代碼
    var SetMiWhidComboStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    function SetMiWhidComboGet(wh_kind) {      
        Ext.Ajax.request({
            url: '/api/CE0044/GetMiWhidCombo',
            method: reqVal_p,
            params: {
                wh_kind: wh_kind
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    SetMiWhidComboStore.removeAll();
                    var MiWhid = data.etts;
                    if (MiWhid.length > 0) {
                        for (var i = 0; i < MiWhid.length; i++) {
                            SetMiWhidComboStore.add({ VALUE: MiWhid[i].VALUE, TEXT: MiWhid[i].TEXT, COMBITEM: MiWhid[i].VALUE + ' ' + MiWhid[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    //責任中心代碼
    var SetUrInidComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0044/GetUrInidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    //藥材類別
    var SetMatClassSubParamComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0044/GetMatClassSubParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P5 = T1QueryForm.getForm().findField('P5');
                if (DataCount > 0) {
                    combo_P5.setValue(store.getAt(0).get('VALUE'));
                    if (menuLink == "AA0209") {
                        combo_P5.setValue('A');
                    }
                    else if (menuLink == "AA0218") {
                        combo_P5.setValue('B');
                    }
                    else if (menuLink == "AB0146") {
                        combo_P5.setValue('A');
                    }
                    else if (menuLink == "AB0153") {
                        combo_P5.setValue('A');
                    }
                    else if (menuLink == "FA0096") {
                        combo_P5.setValue('B');
                    }
                    else if (menuLink == "FA0088") {
                        combo_P5.setValue('A');
                    }
                }
            }
        },
        autoLoad: true
    });
    //藥材代碼
    var SetMiMastComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0044/GetMiMastCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
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
            url: '/api/CE0044/GetContidParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P7 = T1QueryForm.getForm().findField('P7');
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
            url: '/api/CE0044/GetSourcecodeParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P8 = T1QueryForm.getForm().findField('P8');
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
            url: '/api/CE0044/GetWarbakParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P9 = T1QueryForm.getForm().findField('P9');
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
            url: '/api/CE0044/GetRestrictcodeParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P10 = T1QueryForm.getForm().findField('P10');
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
            url: '/api/CE0044/GetCommonParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P11 = T1QueryForm.getForm().findField('P11');
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
            url: '/api/CE0044/GetFastdrugParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P12 = T1QueryForm.getForm().findField('P12');
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
            url: '/api/CE0044/GetDrugkindParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P13 = T1QueryForm.getForm().findField('P13');
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
            url: '/api/CE0044/GetTouchcaseParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P14 = T1QueryForm.getForm().findField('P14');
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
            url: '/api/CE0044/GetOrderkindParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P15 = T1QueryForm.getForm().findField('P15');
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
            url: '/api/CE0044/GetSpecialorderkindParamCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P16 = T1QueryForm.getForm().findField('P16');
                if (DataCount > 0) {
                    combo_P16.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });
    //帶出月結日期區間
    function SetYmDateGet(SET_YM) {
        Ext.Ajax.request({
            url: '/api/CE0044/SetYmDateGet',
            method: reqVal_p,
            params: {
                set_ym: SET_YM
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Query.getForm().findField('SET_BTIME').setValue(data.etts[0].SET_BTIME);
                    T1Query.getForm().findField('SET_CTIME').setValue(data.etts[0].SET_CTIME);
                }
            },
            failure: function (response, options) {
            }
        });
    }
    var mLabelWidth = 70;
    var mWidth = 230;
    // 上方查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        layout: 'vbox',
        border: false,
        //autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
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
            items: [{
                xtype: 'combo',
                store: SetYmComboGet,
                fieldLabel: '月結年月',
                name: 'P0',
                id: 'P0',
                queryMode: 'local',
                labelAlign: 'right',
                fieldCls: 'required',
                allowBlank: false,
                anyMatch: true,
                autoSelect: true,
                limit: 10, //限制一次最多顯示10筆
                displayField: 'SET_YM',
                valueField: 'SET_YM',
                requiredFields: ['SET_YM'],
                width: 150,
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                        SetYmDateGet(records.data.SET_YM);
                    }
                },
                padding: '0 4 0 4'
            }, {
                xtype: 'combo',
                store: SetRlnoComboGet,
                fieldLabel: '盤存單位',
                name: 'P1',
                id: 'P1',
                queryMode: 'local',
                labelAlign: 'right',
                fieldCls: 'required',
                allowBlank: false,
                anyMatch: true,
                autoSelect: true,
                limit: 10, //限制一次最多顯示10筆
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['VALUE'],
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                        if (T1Query.getForm().findField('P1').getValue() == '1') {
                            debugger
                            if (SetWhKindComboGet.find('VALUE', '1') > 0)
                                T1Query.getForm().findField('P2').setValue('1'); // 選擇各請領單位時預設查衛材
                            T1Query.getForm().findField('P2').setDisabled(false);
                            T1Query.getForm().findField('P3').setDisabled(false);
                            T1Query.getForm().findField('P4').setDisabled(false);
                            debugger
                            SetMiWhidComboGet(T1Query.getForm().findField('P2').getValue());

                        }
                        else if (T1Query.getForm().findField('P1').getValue() == '6') {
                            T1Query.getForm().findField('P2').setDisabled(false);
                            T1Query.getForm().findField('P3').setDisabled(false);
                            T1Query.getForm().findField('P4').setDisabled(false);
                            SetMiWhidComboGet(T1Query.getForm().findField('P1').getValue());
                        }
                        else {
                            T1Query.getForm().findField('P2').setDisabled(true);
                            T1Query.getForm().findField('P3').setDisabled(true);
                            T1Query.getForm().findField('P4').setDisabled(true);
                            T1Query.getForm().findField('P2').setValue("");
                            T1Query.getForm().findField('P3').setValue("");
                            T1Query.getForm().findField('P4').setValue("");
                        }

                        if (T1Query.getForm().findField('P1').getValue() == '2' || T1Query.getForm().findField('P1').getValue() == '4') {  
                            T1QueryForm.getForm().findField('P20').setDisabled(false);
                            T1QueryForm.getForm().findField('P21').setDisabled(false);
                        }
                        else {
                            T1QueryForm.getForm().findField('P20').setDisabled(true);
                            T1QueryForm.getForm().findField('P21').setDisabled(true);

                        }


                    }
                },
                padding: '0 4 0 4'
            }, {
                xtype: 'combo',
                store: SetWhKindComboGet,
                fieldLabel: '庫房類別',
                name: 'P2',
                id: 'P2',
                queryMode: 'local',
                labelAlign: 'right',
                fieldCls: 'required',
                allowBlank: false,
                anyMatch: true,
                autoSelect: true,
                limit: 10, //限制一次最多顯示10筆
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['VALUE'],
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>',
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                        if (T1Query.getForm().findField('P1').getValue() != '6')
                            SetMiWhidComboGet(T1Query.getForm().findField('P2').getValue());
                        else
                            SetMiWhidComboGet(T1Query.getForm().findField('P1').getValue());
                    }
                },
                padding: '0 4 0 4'
            }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px;',
            items: [{
                xtype: 'combo',
                store: SetMiWhidComboStore,
                fieldLabel: '庫房代碼',
                name: 'P3',
                id: 'P3',
                queryMode: 'local',
                labelAlign: 'right',
                fieldCls: 'required',
                allowBlank: false,
                anyMatch: true,
                autoSelect: true,
                limit: 10, //限制一次最多顯示10筆
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                requiredFields: ['VALUE'],
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                },
                padding: '0 4 0 4',
                disabled: true
            },
            {
                xtype: 'combo',
                store: SetUrInidComboGet,
                fieldLabel: '責任中心代碼',
                name: 'P4',
                id: 'P4',
                queryMode: 'local',
                labelAlign: 'right',
                labelWidth: 100,
                anyMatch: true,
                autoSelect: true,
                limit: 10, //限制一次最多顯示10筆
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['VALUE'],
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                },
                padding: '0 4 0 4',
                disabled: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '月結日期起',
                name: 'SET_BTIME',
                id: 'SET_BTIME',
                labelWidth: 100,
                width: 180
            },
            {
                xtype: 'displayfield',
                fieldLabel: '月結日期迄',
                name: 'SET_CTIME',
                id: 'SET_CTIME',
                labelWidth: 100,
                width: 180
            }
            ]
        }]
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
                fieldLabel: '藥材類別',
                name: 'P5',
                id: 'P5',
                store: SetMatClassSubParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '藥材代碼',
                name: 'P6',
                id: 'P6',
                store: SetMiMastComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '是否合約',
                name: 'P7',
                id: 'P7',
                store: SetContidParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '買斷寄庫',
                name: 'P8',
                id: 'P8',
                store: SetSourcecodeParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '是否戰備',
                name: 'P9',
                id: 'P9',
                store: SetWarbakParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '管制品項',
                name: 'P10',
                id: 'P10',
                store: SetRestrictcodeParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '是否常用品項',
                name: 'P11',
                id: 'P11',
                store: SetCommonParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '急救品項',
                name: 'P12',
                id: 'P12',
                store: SetFastdrugParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '中西藥別',
                name: 'P13',
                id: 'P13',
                store: SetDrugkindParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '合約類別',
                name: 'P14',
                id: 'P14',
                store: SetTouchcaseParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '採購類別',
                name: 'P15',
                id: 'P15',
                store: SetOrderkindParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'combo',
                fieldLabel: '特殊品項',
                name: 'P16',
                id: 'P16',
                store: SetSpecialorderkindParamComboGet,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>'
            },
            {
                xtype: 'checkboxfield', // 或者 'checkbox'
                fieldLabel: '差異量',
                name: 'P17',
                id: 'P17',
                boxLabel: '差異量不為0'
            },
            {
                xtype: 'checkboxfield',
                boxLabel: '(近6個月進貨) 或 <br>(近6個月醫令耗用) 或 <br>(庫量<>0) 或 <br>(庫存=0且無作廢)',
                name: 'P18',
                id: 'P18',
            },
            {
                xtype: 'checkboxfield',
                boxLabel: '(期初庫存<>0) 或 <br>(期初=0但有進出)',
                name: 'P19',
                id: 'P19',
            },
            {
                xtype: 'checkboxfield',
                boxLabel: '結存不含戰備量',
                disabled: true,
                name: 'P20',
                id: 'P20',
            },
            {
                xtype: 'checkboxfield',
                boxLabel: '結存不足戰備量',
                disabled: true,
                name: 'P21',
                id: 'P21',
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                if (T1Query.getForm().isValid()) {
                    var f = T1Query.getForm();
                    objMask.show();
                    T1Load();
                    T2Load();
                }
                else {
                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>請輸入必填欄位</span>');
                }
                msglabel('訊息區:');
            }
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                T1Query.getForm().findField('P0').setValue(p0_default);
                T1Query.getForm().findField('SET_BTIME').setValue(p0_default2);
                T1Query.getForm().findField('SET_CTIME').setValue(p0_default3);
                T1Query.getForm().findField('P1').setValue('1');
                T1Query.getForm().findField('P2').setValue(' ');
                T1QueryForm.getForm().findField('P7').setValue(' ');
                T1QueryForm.getForm().findField('P8').setValue(' ');
                T1QueryForm.getForm().findField('P9').setValue(' ');
                T1QueryForm.getForm().findField('P10').setValue(' ');
                T1QueryForm.getForm().findField('P11').setValue(' ');
                T1QueryForm.getForm().findField('P12').setValue(' ');
                T1QueryForm.getForm().findField('P13').setValue(' ');
                T1QueryForm.getForm().findField('P14').setValue(' ');
                T1QueryForm.getForm().findField('P15').setValue(' ');
                T1QueryForm.getForm().findField('P16').setValue(' ');
                T1QueryForm.getForm().findField('P17').setValue(' ');
                T1QueryForm.getForm().findField('P18').setValue(' ');
                T1QueryForm.getForm().findField('P19').setValue(' ');
                T1QueryForm.getForm().findField('P20').setValue(' ');
                T1QueryForm.getForm().findField('P21').setValue(' ');
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                msglabel('訊息區:');
            }
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.CE0044M', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    P0: T1Query.getForm().findField('P0').getValue(),
                    P1: T1Query.getForm().findField('P1').getValue(),
                    P2: T1Query.getForm().findField('P2').getValue(),
                    P3: T1Query.getForm().findField('P3').getValue(),
                    P4: T1Query.getForm().findField('P4').getValue(),
                    P5: T1QueryForm.getForm().findField('P5').getValue(),
                    P6: T1QueryForm.getForm().findField('P6').getValue(),
                    P7: T1QueryForm.getForm().findField('P7').getValue(),
                    P8: T1QueryForm.getForm().findField('P8').getValue(),
                    P9: T1QueryForm.getForm().findField('P9').getValue(),
                    P10: T1QueryForm.getForm().findField('P10').getValue(),
                    P11: T1QueryForm.getForm().findField('P11').getValue(),
                    P12: T1QueryForm.getForm().findField('P12').getValue(),
                    P13: T1QueryForm.getForm().findField('P13').getValue(),
                    P14: T1QueryForm.getForm().findField('P14').getValue(),
                    P15: T1QueryForm.getForm().findField('P15').getValue(),
                    P16: T1QueryForm.getForm().findField('P16').getValue(),
                    P17: T1QueryForm.getForm().findField('P17').getValue(),
                    P18: T1QueryForm.getForm().findField('P18').getValue(), // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
                    P19: T1QueryForm.getForm().findField('P19').getValue(), // (期初庫存<>0)或(期初=0但有進出)
                    P20: T1QueryForm.getForm().findField('P20').getValue(),
                    P21: T1QueryForm.getForm().findField('P21').getValue()
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
                        T1Tool.down('#export_805').setDisabled(false);
                        //T1Tool.down('#export_803_detail').setDisabled(false);
                        T1Tool.down('#export_csv').setDisabled(false);
                        T1LastRec = records[0]; // 不論資料有幾筆,T1LastRec先設為第一筆
                    }
                    else {
                        T1Tool.down('#export').setDisabled(true);
                        T1Tool.down('#export_805').setDisabled(true);
                        //T1Tool.down('#export_803_detail').setDisabled(true);
                        T1Tool.down('#export_csv').setDisabled(true);
                        msglabel('查無資料!');
                        Ext.Msg.alert('提醒', '查無資料!');
                    }
                }
            }
        }
    });
    var T2Store = Ext.create('WEBAPP.store.CE0044D', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    P0: T1Query.getForm().findField('P0').getValue(),
                    P1: T1Query.getForm().findField('P1').getValue(),
                    P2: T1Query.getForm().findField('P2').getValue(),
                    P3: T1Query.getForm().findField('P3').getValue(),
                    P4: T1Query.getForm().findField('P4').getValue(),
                    P5: T1QueryForm.getForm().findField('P5').getValue(),
                    P6: T1QueryForm.getForm().findField('P6').getValue(),
                    P7: T1QueryForm.getForm().findField('P7').getValue(),
                    P8: T1QueryForm.getForm().findField('P8').getValue(),
                    P9: T1QueryForm.getForm().findField('P9').getValue(),
                    P10: T1QueryForm.getForm().findField('P10').getValue(),
                    P11: T1QueryForm.getForm().findField('P11').getValue(),
                    P12: T1QueryForm.getForm().findField('P12').getValue(),
                    P13: T1QueryForm.getForm().findField('P13').getValue(),
                    P14: T1QueryForm.getForm().findField('P14').getValue(),
                    P15: T1QueryForm.getForm().findField('P15').getValue(),
                    P16: T1QueryForm.getForm().findField('P16').getValue(),
                    P17: T1QueryForm.getForm().findField('P17').getValue(),
                    P18: T1QueryForm.getForm().findField('P18').getValue(),
                    P19: T1QueryForm.getForm().findField('P19').getValue(),
                    P20: T1QueryForm.getForm().findField('P20').getValue(),
                    P21: T1QueryForm.getForm().findField('P21').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T2Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T2Form.loadRecord(records[0]);
                        objMask.hide();
                    }
                    else {
                        msglabel('查無資料!');
                        Ext.Msg.alert('提醒', '查無資料!');
                        T2Form.getForm().reset();
                    }
                }
            }
        }
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
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p5', value: T1QueryForm.getForm().findField('P5').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p6', value: T1QueryForm.getForm().findField('P6').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p7', value: T1QueryForm.getForm().findField('P7').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p8', value: T1QueryForm.getForm().findField('P8').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p9', value: T1QueryForm.getForm().findField('P9').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p10', value: T1QueryForm.getForm().findField('P10').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p11', value: T1QueryForm.getForm().findField('P11').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p12', value: T1QueryForm.getForm().findField('P12').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p13', value: T1QueryForm.getForm().findField('P13').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p14', value: T1QueryForm.getForm().findField('P14').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p15', value: T1QueryForm.getForm().findField('P15').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p16', value: T1QueryForm.getForm().findField('P16').getValue() }); //SQL篩選條件
                            p.push({ name: 'p17', value: T1QueryForm.getForm().findField('P17').getValue() }); //SQL篩選條件
                            p.push({ name: 'p18', value: T1QueryForm.getForm().findField('P18').getValue() }); // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
                            p.push({ name: 'p19', value: T1QueryForm.getForm().findField('P19').getValue() }); // (期初庫存<>0)或(期初=0但有進出)
                            p.push({ name: 'p20', value: T1QueryForm.getForm().findField('P20').getValue() });
                            p.push({ name: 'p21', value: T1QueryForm.getForm().findField('P21').getValue() });
                            p.push({ name: 'hosp_code', value: hosp_code }); //SQL篩選條件
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            },
            {
                itemId: 'export_csv', text: '匯出CSV', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p5', value: T1QueryForm.getForm().findField('P5').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p6', value: T1QueryForm.getForm().findField('P6').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p7', value: T1QueryForm.getForm().findField('P7').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p8', value: T1QueryForm.getForm().findField('P8').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p9', value: T1QueryForm.getForm().findField('P9').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p10', value: T1QueryForm.getForm().findField('P10').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p11', value: T1QueryForm.getForm().findField('P11').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p12', value: T1QueryForm.getForm().findField('P12').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p13', value: T1QueryForm.getForm().findField('P13').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p14', value: T1QueryForm.getForm().findField('P14').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p15', value: T1QueryForm.getForm().findField('P15').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p16', value: T1QueryForm.getForm().findField('P16').getValue() }); //SQL篩選條件
                            p.push({ name: 'p17', value: T1QueryForm.getForm().findField('P17').getValue() }); //SQL篩選條件
                            p.push({ name: 'p18', value: T1QueryForm.getForm().findField('P18').getValue() }); // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
                            p.push({ name: 'p19', value: T1QueryForm.getForm().findField('P19').getValue() }); // (期初庫存<>0)或(期初=0但有進出)
                            p.push({ name: 'p20', value: T1QueryForm.getForm().findField('P20').getValue() });
                            p.push({ name: 'p21', value: T1QueryForm.getForm().findField('P21').getValue() });
                            p.push({ name: 'hosp_code', value: hosp_code }); //SQL篩選條件
                            PostForm(T1GetCsv, p);
                        }
                    });
                }
            },
            {
                text: '消耗結存月報表',
                id: 'btnPrint',
                name: 'btnPrint',
                handler: function () {
                    //if (T1Query.getForm().isValid()) {
                    if (T1Query.getForm().findField('P0').getValue() != '') {

                        showReport();
                    }
                    else {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>請輸入必填欄位</span>');
                    }
                }
            },
            {
                text: '消耗結存總表',
                id: 'btnPrint2',
                name: 'btnPrint2',
                handler: function () {
                    if (T1Query.getForm().findField('P0').getValue() != '') {
                        showReport2();
                    }
                    else {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>請輸入月結年月</span>');
                    }
                }
            },
            {
                itemId: 'export_805', text: '匯出消耗結存表', disabled: true, hidden: true,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p5', value: T1QueryForm.getForm().findField('P5').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p6', value: T1QueryForm.getForm().findField('P6').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p7', value: T1QueryForm.getForm().findField('P7').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p8', value: T1QueryForm.getForm().findField('P8').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p9', value: T1QueryForm.getForm().findField('P9').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p10', value: T1QueryForm.getForm().findField('P10').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p11', value: T1QueryForm.getForm().findField('P11').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p12', value: T1QueryForm.getForm().findField('P12').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p13', value: T1QueryForm.getForm().findField('P13').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p14', value: T1QueryForm.getForm().findField('P14').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p15', value: T1QueryForm.getForm().findField('P15').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p16', value: T1QueryForm.getForm().findField('P16').getValue() }); //SQL篩選條件
                            p.push({ name: 'p17', value: T1QueryForm.getForm().findField('P17').getValue() }); //SQL篩選條件
                            p.push({ name: 'p18', value: T1QueryForm.getForm().findField('P18').getValue() }); //SQL篩選條件
                            p.push({ name: 'p19', value: T1QueryForm.getForm().findField('P19').getValue() }); //SQL篩選條件
                            p.push({ name: 'p20', value: T1QueryForm.getForm().findField('P20').getValue() }); //SQL篩選條件
                            p.push({ name: 'p21', value: T1QueryForm.getForm().findField('P21').getValue() }); //SQL篩選條件
                            p.push({ name: 'hosp_code', value: hosp_code }); //SQL篩選條件
                            PostForm(T1GetExcel_805, p);
                        }
                    });
                }
            },
            {
                itemId: 'export_803_detail', text: '匯出消耗結存明細', disabled: false, hidden: true,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p5', value: T1QueryForm.getForm().findField('P5').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p6', value: T1QueryForm.getForm().findField('P6').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p7', value: T1QueryForm.getForm().findField('P7').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p8', value: T1QueryForm.getForm().findField('P8').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p9', value: T1QueryForm.getForm().findField('P9').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p10', value: T1QueryForm.getForm().findField('P10').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p11', value: T1QueryForm.getForm().findField('P11').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p12', value: T1QueryForm.getForm().findField('P12').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p13', value: T1QueryForm.getForm().findField('P13').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p14', value: T1QueryForm.getForm().findField('P14').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p15', value: T1QueryForm.getForm().findField('P15').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p16', value: T1QueryForm.getForm().findField('P16').getValue() }); //SQL篩選條件
                            p.push({ name: 'p17', value: T1QueryForm.getForm().findField('P17').getValue() }); //SQL篩選條件
                            p.push({ name: 'p18', value: T1QueryForm.getForm().findField('P18').getValue() }); //SQL篩選條件
                            p.push({ name: 'p19', value: T1QueryForm.getForm().findField('P19').getValue() }); //SQL篩選條件
                            p.push({ name: 'p20', value: T1QueryForm.getForm().findField('P20').getValue() }); //SQL篩選條件
                            p.push({ name: 'p21', value: T1QueryForm.getForm().findField('P21').getValue() }); //SQL篩選條件
                            p.push({ name: 'hosp_code', value: hosp_code }); //SQL篩選條件
                            PostForm(T1GetExcel_803Detail, p);
                        }
                    });
                }
            },
            {
                text: '消耗結存月報表(金額)', disabled: false, hidden: true,
                id: 'btnPrint_803',
                name: 'btnPrint_803',
                handler: function () {
                    //if (T1Query.getForm().isValid()) {
                    if (T1Query.getForm().findField('P0').getValue() != '') {

                        showReport3();
                    }
                    else {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>請輸入必填欄位</span>');
                    }
                }
            }
        ]
    });
    function T1Load() {
        if ((T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null) ||
            (T1Query.getForm().findField('P1').getValue() == "" || T1Query.getForm().findField('P1').getValue() == null)) {
            Ext.Msg.alert('訊息', '需填月結年月、盤存單位才能查詢');
        } else {
            //各請領單位需填庫房類別
            if (T1Query.getForm().findField('P1').getValue == "1") {
                if (T1Query.getForm().findField('P2').getValue() == "" || T1Query.getForm().findField('P2').getValue() == null) {
                    Ext.Msg.alert('訊息', '需填庫房類別才能查詢');
                } else {
                    T1Store.load({
                        params: {
                            start: 0
                        }
                    });
                    T1Tool.moveFirst();
                }
            } else {
                T1Store.load({
                    params: {
                        start: 0
                    }
                });
                T1Tool.moveFirst();
            }
        }
        msglabel('訊息區:');
    }
    function T2Load() {
        T2Store.load({
            params: {
                start: 0
            }
        });
    }

    // 查詢結果資料列表(上半部)
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
        columns: [
            { xtype: 'rownumberer', width: 60 },
            { text: "盤存單位", dataIndex: "F1", width: 85, locked: true },
            { text: "藥材代碼", dataIndex: "F2", width: 85, locked: true },
            { text: "廠商代碼", dataIndex: "F3", width: 85, locked: true },
            { text: "藥材名稱", dataIndex: "F4", width: 85, locked: true },
            { text: "英文品名", dataIndex: "F61", width: 85, locked: true },
            { text: "藥材單位", dataIndex: "F5", width: 85, locked: true },
            { text: "單價", dataIndex: "F6", width: 85 },
            { text: "優惠後單價", dataIndex: "F7", width: 85 },
            { text: "上月單價", dataIndex: "F8", width: 85 },
            { text: "上月優惠後單價", dataIndex: "F9", width: 85 },
            { text: "包裝量", dataIndex: "F10", width: 85 },
            { text: "上月結存", dataIndex: "F11", width: 85 },
            { text: "進貨入庫", dataIndex: "F12_1", width: 85 },
            { text: "撥發入庫", dataIndex: "F12_2", width: 85 },
            { text: "撥發出庫", dataIndex: "F13", width: 85 },
            { text: "調撥入庫", dataIndex: "F14", width: 85 },
            { text: "調撥出庫", dataIndex: "F15", width: 85 },
            { text: "調帳入庫", dataIndex: "F16", width: 85 },
            { text: "調帳出庫", dataIndex: "F17", width: 85 },
           // { text: "繳回入庫", dataIndex: "F18", width: 85 },
           // { text: "繳回出庫", dataIndex: "F19", width: 85 },
            { text: "退料入庫", dataIndex: "F56", width: 85 },
            { text: "退料出庫", dataIndex: "F57", width: 85 },
            { text: "退貨量", dataIndex: "F20", width: 85 },
            { text: "報廢量", dataIndex: "F21", width: 85 },
            { text: "換貨入庫", dataIndex: "F22", width: 85 },
            { text: "換貨出庫", dataIndex: "F23", width: 85 },
            { text: "軍方消耗", dataIndex: "F24", width: 85 },
            { text: "民眾消耗", dataIndex: "F25", width: 85 },
            { text: "本月總消耗", dataIndex: "F26", width: 85 },
            { text: "應有結存", dataIndex: "F27", width: 85 },
            { text: "盤存量", dataIndex: "F28", width: 85 },
            { text: "本月結存", dataIndex: "F29", width: 85 },
            { text: "上月寄庫藥品買斷結存", dataIndex: "F30", width: 85 },
            { text: "本月寄庫藥品買斷結存", dataIndex: "F31", width: 85 },
            { text: "差異量", dataIndex: "F32", width: 85 },
            { text: "結存金額", dataIndex: "F33", width: 85, hidden: showMoney() },
            { text: "優惠後結存金額", dataIndex: "F34", width: 85, hidden: showMoney() },
            { text: "差異金額", dataIndex: "F35", width: 85, hidden: showMoney() },
            { text: "優惠後差異金額", dataIndex: "F36", width: 85, hidden: showMoney() },
            { text: "藥材類別", dataIndex: "F37", width: 85 },
            { text: "是否合約", dataIndex: "F38", width: 85 },
            { text: "買斷寄庫", dataIndex: "F39", width: 85 },
            { text: "是否戰備", dataIndex: "F40", width: 85 },
            { text: "戰備存量", dataIndex: "F41", width: 85 },
            { text: "期末比", dataIndex: "F42", width: 85 },
            { text: "贈品數量", dataIndex: "F43", width: 85 },
            { text: "上月戰備量", dataIndex: "F44", width: 85 },
            { text: "上月是否戰備", dataIndex: "F45", width: 85 },
            { text: "不含戰備上月結存", dataIndex: "F46", width: 85 },
            { text: "單價差額", dataIndex: "F47", width: 85 },
            { text: "不含戰備上月結存價差", dataIndex: "F48", width: 85 },
            { text: "戰備本月價差", dataIndex: "F49", width: 85 },
            { text: "不含戰備本月結存", dataIndex: "F50", width: 85 },
            { text: "不含戰備本月應有結存量", dataIndex: "F51", width: 85 },
            { text: "不含戰備本月盤存量", dataIndex: "F52", width: 85 },
            { text: "不含贈品本月進貨", dataIndex: "F53", width: 85 },
            { text: "單位申請基準量", dataIndex: "F54", width: 85 },
            { text: "下月預計申請量", dataIndex: "F55", width: 85 },
            { text: "本月單價5000元以上總消耗", dataIndex: "F58", width: 85 },
            { text: "本月單價未滿5000元總消耗", dataIndex: "F59", width: 85 },
            { text: "合約案號", dataIndex: "F60", width: 85 },
            //{ text: "上月結存總金額", dataIndex: "T01", width: 85 },
            //{ text: "本月進貨總金額", dataIndex: "T02", width: 85 },
            //{ text: "本月退貨總金額", dataIndex: "T03", width: 85 },
            //{ text: "本月贈品總金額", dataIndex: "T04", width: 85 },
            //{ text: "消耗金額總金額", dataIndex: "T05", width: 85 },
            //{ text: "軍方消耗總金額", dataIndex: "T06", width: 85 },
            //{ text: "民眾消耗總金額", dataIndex: "T07", width: 85 },
            //{ text: "退料總金額", dataIndex: "T08", width: 85 },
            //{ text: "調撥總金額", dataIndex: "T09", width: 85 },
            //{ text: "換貨總金額", dataIndex: "T10", width: 85 },
            //{ text: "報廢總金額", dataIndex: "T11", width: 85 },
            //{ text: "應有結存總金額", dataIndex: "T12", width: 85 },
            //{ text: "盤點結存總金額", dataIndex: "T13", width: 85 },
            //{ text: "本月結存總金額", dataIndex: "T14", width: 85 },
            //{ text: "買斷結存總金額", dataIndex: "T15", width: 85 },
            //{ text: "上月寄庫藥品買斷結存總金額", dataIndex: "T16", width: 85 },
            //{ text: "本月寄庫藥品買斷結存總金額", dataIndex: "T17", width: 85 },
            //{ text: "戰備金額總金額", dataIndex: "T18", width: 85 },
            //{ text: "寄庫結存總金額", dataIndex: "T19", width: 85 },
            //{ text: "不含戰備上月結存價差金額", dataIndex: "T20", width: 85 },
            //{ text: "戰備本月價差金額", dataIndex: "T21", width: 85 },
            //{ text: "折讓總金額", dataIndex: "T22", width: 85 },
            //{ text: "進貨總金額", dataIndex: "T23", width: 85 },
            //{ text: "盤盈總金額", dataIndex: "T24", width: 85 },
            //{ text: "盤虧總金額", dataIndex: "T25", width: 85 },
            //{ text: "合計盤盈虧總金額", dataIndex: "T26", width: 85 },
            //{ text: "本月單價5000元以上消耗總金額", dataIndex: "T27", width: 85 },
            //{ text: "本月單價未滿5000元消耗總金額", dataIndex: "T28", width: 85 },
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
    // 查詢結果資料列表(下半部)
    var T2Form = Ext.create('Ext.form.Panel', {
        xtype: 'form',
        id: 'T2Form',
        bodyStyle: 'padding:5px 5px 0',
        width: 1500,
        layout: {
            type: 'table',
            columns: 4,
            border: true,
            bodyBorder: true,
            tdAttrs: { width: '25%' }
        },
        bodyPadding: '5 5 0 0',
        autoScroll: true,
        frame: false,
        defaults: {
            labelAlign: 'left',
            readOnly: true,
            labelWidth: mLabelWidth,
            width: mWidth,
            padding: '4 0 4 20',
            msgTarget: 'side'
        },
        defaultType: 'textfield',
        items: [
            { xtype: 'displayfield', value: '各項總金額合計：', readOnly: true, fieldStyle: 'font-weight: bold;font-size: 20px;', padding: '4 0 4 20' },
            { fieldLabel: '合計進貨金額', name: 'F23', readOnly: true, width: mWidth, labelWidth: 90, colspan: 3, hidden: showMoney() },

            { fieldLabel: '上月結存', name: 'F1', readOnly: true, width: mWidth },
            { fieldLabel: '本月進貨', name: 'F2', readOnly: true, width: mWidth },
            { fieldLabel: '本月退貨', name: 'F3', readOnly: true, width: mWidth },
            { fieldLabel: '本月贈品', name: 'F4', readOnly: true, width: mWidth },

            { fieldLabel: '撥發入庫金額', name: 'F29', readOnly: true, width: mWidth },
            { fieldLabel: '撥發出庫金額', name: 'F30', readOnly: true, width: mWidth, colspan: 3},

            { fieldLabel: '消耗金額', name: 'F5', readOnly: true, width: mWidth, hidden: showMoney() },
            { fieldLabel: '軍方消耗', name: 'F6', readOnly: true, width: mWidth },
            { fieldLabel: '民眾消耗', name: 'F7', readOnly: true, width: mWidth, colspan: 2},

            { fieldLabel: '退料金額', name: 'F8', readOnly: true, width: mWidth, hidden: showMoney() },
            { fieldLabel: '調撥金額', name: 'F9', readOnly: true, width: mWidth, hidden: showMoney() },
            { fieldLabel: '換貨金額', name: 'F10', readOnly: true, width: mWidth, hidden: showMoney() },
            { fieldLabel: '報廢金額', name: 'F11', readOnly: true, width: mWidth, hidden: showMoney() },

            { fieldLabel: '應有結存', name: 'F12', readOnly: true, width: mWidth },
            { fieldLabel: '盤點結存', name: 'F13', readOnly: true, width: mWidth },
            { fieldLabel: '本月結存', name: 'F14', readOnly: true, width: mWidth, colspan: 2 },

            { fieldLabel: '盤盈金額', name: 'F24', readOnly: true, width: mWidth, hidden: showMoney() },
            { fieldLabel: '盤虧金額', name: 'F25', readOnly: true, width: mWidth, hidden: showMoney() },
            { fieldLabel: '合計盤盈虧總金額', name: 'F26', readOnly: true, width: mWidth, colspan: 2, labelWidth: 90, hidden: showMoney() },
            //{ fieldLabel: '差異金額', name: 'F15', readOnly: true, width: mWidth, hidden: showMoney() },

            { fieldLabel: '買斷結存', name: 'F15', readOnly: true, width: mWidth },
            { fieldLabel: '上月寄庫藥品買斷結存', name: 'F16', readOnly: true, width: mWidth, labelWidth: 90 },
            { fieldLabel: '本月寄庫藥品買斷結存', name: 'F17', readOnly: true, width: mWidth, labelWidth: 90 },
            { fieldLabel: '戰備金額', name: 'F18', readOnly: true, width: mWidth, hidden: showMoney() },

            { fieldLabel: '寄庫結存', name: 'F19', readOnly: true, width: mWidth },
            { fieldLabel: '不含戰備上月結存價差', name: 'F20', readOnly: true, width: mWidth, labelWidth: 90 },
            { fieldLabel: '戰備本月價差', name: 'F21', readOnly: true, width: mWidth, labelWidth: 90 },
            { fieldLabel: '折讓金額', name: 'F22', readOnly: true, width: mWidth, hidden: showMoney() }
            //{
            //    xtype: 'panel',
            //    border: false,
            //    layout: 'vbox',
            //    width: 350,
            //    bodyStyle: 'padding: 3px 5px;',
            //    defaults: {
            //        xtype: 'textfield',
            //        labelAlign: 'left',
            //        readOnly: true,
            //        labelWidth: 50,
            //        width: mWidth,
            //        padding: '4 0 4 20',
            //        msgTarget: 'side',
            //        readOnly: true
            //    },
            //    items: [
            //        {
            //            xtype: 'displayfield', value: '日平均消耗',
            //            fieldStyle: 'font-weight: bold;color: #0043ff;font-size: 20px;', padding: '20 0 4 20'
            //        },
            //        { fieldLabel: '10天', name: 'USE_QTY_10' },
            //        { fieldLabel: '14天', name: 'USE_QTY_14' },
            //        { fieldLabel: '90天', name: 'USE_QTY_90' }
            //    ]
            //},
            //{
            //    xtype: 'panel',
            //    border: false,
            //    layout: 'vbox',
            //    width: 300,
            //    bodyStyle: 'padding: 3px 5px;',
            //    defaults: {
            //        xtype: 'textfield',
            //        labelAlign: 'left',
            //        readOnly: true,
            //        labelWidth: 80,
            //        width: mWidth,
            //        padding: '4 0 4 20',
            //        msgTarget: 'side',
            //        readOnly: true
            //    },
            //    items: [
            //        {
            //            xtype: 'displayfield', value: '前六個月消耗',
            //            fieldStyle: 'font-weight: bold;color: #0043ff;font-size: 20px;', padding: '20 0 4 20'
            //        },
            //        { fieldLabel: '前第一個月', name: 'USE_QTY_1M' },
            //        { fieldLabel: '前第二個月', name: 'USE_QTY_2M' },
            //        { fieldLabel: '前第三個月', name: 'USE_QTY_3M' },
            //        { fieldLabel: '前第四個月', name: 'USE_QTY_4M' },
            //        { fieldLabel: '前第五個月', name: 'USE_QTY_5M' },
            //        { fieldLabel: '前第六個月', name: 'USE_QTY_6M' },
            //        { fieldLabel: '三個月平均', name: 'USE_QTY_3MA' },
            //        { fieldLabel: '六個月平均', name: 'USE_QTY_6MA' }
            //    ]
            //}
        ]
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + T1Query.getForm().findField('P0').getValue()
                + '&p1=' + T1Query.getForm().findField('P1').getValue()
                + '&p2=' + T1Query.getForm().findField('P2').getValue()
                + '&p3=' + T1Query.getForm().findField('P3').getValue()
                + '&p4=' + T1Query.getForm().findField('P4').getValue()
                + '&p5=' + T1QueryForm.getForm().findField('P5').getValue()
                + '&p6=' + T1QueryForm.getForm().findField('P6').getValue()
                + '&p7=' + T1QueryForm.getForm().findField('P7').getValue()
                + '&p8=' + T1QueryForm.getForm().findField('P8').getValue()
                + '&p9=' + T1QueryForm.getForm().findField('P9').getValue()
                + '&p10=' + T1QueryForm.getForm().findField('P10').getValue()
                + '&p11=' + T1QueryForm.getForm().findField('P11').getValue()
                + '&p12=' + T1QueryForm.getForm().findField('P12').getValue()
                + '&p13=' + T1QueryForm.getForm().findField('P13').getValue()
                + '&p14=' + T1QueryForm.getForm().findField('P14').getValue()
                + '&p15=' + T1QueryForm.getForm().findField('P15').getValue()
                + '&p16=' + T1QueryForm.getForm().findField('P16').getValue()
                + '&p17=' + T1QueryForm.getForm().findField('P17').getValue()
                + '&p18=' + T1QueryForm.getForm().findField('P18').getValue()
                + '&p19=' + T1QueryForm.getForm().findField('P19').getValue()
                + '&p20=' + T1QueryForm.getForm().findField('P20').getValue()
                + '&p21=' + T1QueryForm.getForm().findField('P21').getValue()
                + '&wh_name=' + T1Query.getForm().findField('P3').rawValue
                + '&report=1'
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

    function showReport2() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + T1Query.getForm().findField('P0').getValue()
                + '&p1=' + T1Query.getForm().findField('P1').getValue()
                + '&p2=' + T1Query.getForm().findField('P2').getValue()
                + '&p3=' + T1Query.getForm().findField('P3').getValue()
                + '&p4=' + T1Query.getForm().findField('P4').getValue()
                + '&p5=' + T1QueryForm.getForm().findField('P5').getValue()
                + '&p6=' + T1QueryForm.getForm().findField('P6').getValue()
                + '&p7=' + T1QueryForm.getForm().findField('P7').getValue()
                + '&p8=' + T1QueryForm.getForm().findField('P8').getValue()
                + '&p9=' + T1QueryForm.getForm().findField('P9').getValue()
                + '&p10=' + T1QueryForm.getForm().findField('P10').getValue()
                + '&p11=' + T1QueryForm.getForm().findField('P11').getValue()
                + '&p12=' + T1QueryForm.getForm().findField('P12').getValue()
                + '&p13=' + T1QueryForm.getForm().findField('P13').getValue()
                + '&p14=' + T1QueryForm.getForm().findField('P14').getValue()
                + '&p15=' + T1QueryForm.getForm().findField('P15').getValue()
                + '&p16=' + T1QueryForm.getForm().findField('P16').getValue()
                + '&p17=' + T1QueryForm.getForm().findField('P17').getValue()
                + '&p18=' + T1QueryForm.getForm().findField('P18').getValue()
                + '&p19=' + T1QueryForm.getForm().findField('P19').getValue()
                + '&p20=' + T1QueryForm.getForm().findField('P20').getValue()
                + '&p21=' + T1QueryForm.getForm().findField('P21').getValue()
                + '&wh_name=' + T1Query.getForm().findField('P3').rawValue
                + '&report=2'
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

    function showReport3() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + T1Query.getForm().findField('P0').getValue()
                + '&p1=' + T1Query.getForm().findField('P1').getValue()
                + '&p2=' + T1Query.getForm().findField('P2').getValue()
                + '&p3=' + T1Query.getForm().findField('P3').getValue()
                + '&p4=' + T1Query.getForm().findField('P4').getValue()
                + '&p5=' + T1QueryForm.getForm().findField('P5').getValue()
                + '&p6=' + T1QueryForm.getForm().findField('P6').getValue()
                + '&p7=' + T1QueryForm.getForm().findField('P7').getValue()
                + '&p8=' + T1QueryForm.getForm().findField('P8').getValue()
                + '&p9=' + T1QueryForm.getForm().findField('P9').getValue()
                + '&p10=' + T1QueryForm.getForm().findField('P10').getValue()
                + '&p11=' + T1QueryForm.getForm().findField('P11').getValue()
                + '&p12=' + T1QueryForm.getForm().findField('P12').getValue()
                + '&p13=' + T1QueryForm.getForm().findField('P13').getValue()
                + '&p14=' + T1QueryForm.getForm().findField('P14').getValue()
                + '&p15=' + T1QueryForm.getForm().findField('P15').getValue()
                + '&p16=' + T1QueryForm.getForm().findField('P16').getValue()
                + '&p17=' + T1QueryForm.getForm().findField('P17').getValue()
                + '&p18=' + T1QueryForm.getForm().findField('P18').getValue()
                + '&p19=' + T1QueryForm.getForm().findField('P19').getValue()
                + '&p20=' + T1QueryForm.getForm().findField('P20').getValue()
                + '&p21=' + T1QueryForm.getForm().findField('P21').getValue()
                + '&wh_name=' + T1Query.getForm().findField('P3').rawValue
                + '&report=3'
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
                    id: 'T1GridNorth',
                    collapsible: false,
                    title: '',
                    split: true,
                    height: '60%',
                    items: [T1Grid]
                },
                {
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    title: '',
                    height: '40%',
                    split: true,
                    items: [T2Form]
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
    var objMask = new Ext.LoadMask({
        target: T2Form,
        msg: '讀取中...',
        hideMode: 'display',
        listeners: {
            beforedestroy: function (lmask) {
                //this.hide();
            },
            hide: function (lmask) {
                //this.hide();
            }
        }
    });
    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();

    var st_getlogininfo = Ext.create('Ext.data.Store', {
        listeners: {
            load: function (store, eOpts) {
                hosp_code = store.getAt(0).get('HOSP_CODE');
                //hosp_code = '805'; // test
                // 依取得的HOSP_CODE處理欄位
                if (hosp_code != '805') {
                    // 不為805(花蓮)則不顯示[單位申請基準量] 與 [下月預計申請量]
                    setVisibleColumns();
                }
                if (hosp_code == '805') {
                    // 增加[匯出消耗結存表] 此按鈕為805(花蓮)專用
                    T1Grid.down('#export_805').setVisible(true);
                }
                if (hosp_code == '803') {
                    // 增加[匯出消耗結存明細] 此按鈕為803(台中)專用
                    T1Grid.down('#export_803_detail').setVisible(true);
                    T1Grid.down('#btnPrint_803').setVisible(true);
                }
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0044/GetLoginInfo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    // 設定欄位是否顯示
    var setVisibleColumns = function () {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        T1Grid.suspendLayouts();
        //var columnManager = T1Grid.getColumnManager();

        //columnManager.getColumnByDataIndex("F54").setHidden(true);
        //columnManager.getColumnByDataIndex("F55").setHidden(true);
        var columns = T1Grid.columns;

        var columnToHide = null;
        Ext.Array.each(columns, function (column) {
            if (column.dataIndex == 'F54' || column.dataIndex == 'F55') { // 要找的 dataIndex
                column.setHidden(true);
            }
        });


        T1Grid.resumeLayouts(true);

        myMask.hide();
    };

    function MenuLinkSet() {
        if (menuLink == "AB0146" || menuLink == "FA0089") {
            Ext.getCmp('T2Form').hide();
            Ext.getCmp('T1GridNorth').height = '100%';
        }
        else if (menuLink == "CE0047" || menuLink == "CE0042") {
            Ext.getCmp('T2Form').hide();
            Ext.getCmp('T1GridNorth').height = '100%';
            T1QueryForm.getForm().findField('P17').setValue(true);
            var columns = T1Grid.columns;
            Ext.Array.each(columns, function (column) {
                if (column.dataIndex == 'F3'
                    || column.dataIndex == 'F6'
                    || column.dataIndex == 'F7'
                    || column.dataIndex == 'F8'
                    || column.dataIndex == 'F9'
                    || column.dataIndex == 'F10'
                    || column.dataIndex == 'F11'
                    || column.dataIndex == 'F12_1'
                    || column.dataIndex == 'F12_2'
                    || column.dataIndex == 'F13'
                    || column.dataIndex == 'F14'
                    || column.dataIndex == 'F15'
                    || column.dataIndex == 'F16'
                    || column.dataIndex == 'F17'
                    || column.dataIndex == 'F18'
                    || column.dataIndex == 'F19'
                    || column.dataIndex == 'F20'
                    || column.dataIndex == 'F21'
                    || column.dataIndex == 'F22'
                    || column.dataIndex == 'F23'
                    || column.dataIndex == 'F24'
                    || column.dataIndex == 'F25'
                    || column.dataIndex == 'F26'
                    || column.dataIndex == 'F29'
                    || column.dataIndex == 'F30'
                    || column.dataIndex == 'F31'
                    || column.dataIndex == 'F33'
                    || column.dataIndex == 'F34'
                    || column.dataIndex == 'F35'
                    || column.dataIndex == 'F36'
                    || column.dataIndex == 'F37'
                    || column.dataIndex == 'F38'
                    || column.dataIndex == 'F39'
                    || column.dataIndex == 'F40'
                    || column.dataIndex == 'F41'
                    || column.dataIndex == 'F42'
                    || column.dataIndex == 'F43'
                    || column.dataIndex == 'F44'
                    || column.dataIndex == 'F45'
                    || column.dataIndex == 'F46'
                    || column.dataIndex == 'F47'
                    || column.dataIndex == 'F48'
                    || column.dataIndex == 'F49'
                    || column.dataIndex == 'F50'
                    || column.dataIndex == 'F51'
                    || column.dataIndex == 'F52'
                    || column.dataIndex == 'F53'
                    || column.dataIndex == 'F54'
                    || column.dataIndex == 'F55'
                ) {
                    column.setHidden(true);
                }
            });
            T1Grid.resumeLayouts(true);
        }
        else if (menuLink == "CE0043") {
            T1QueryForm.getForm().findField('P17').setValue(true);
            var columns = T1Grid.columns;
            Ext.Array.each(columns, function (column) {
                if (column.dataIndex == 'F3'
                    || column.dataIndex == 'F6'
                    || column.dataIndex == 'F7'
                    || column.dataIndex == 'F8'
                    || column.dataIndex == 'F9'
                    || column.dataIndex == 'F10'
                    || column.dataIndex == 'F11'
                    || column.dataIndex == 'F12_1'
                    || column.dataIndex == 'F12_2'
                    || column.dataIndex == 'F13'
                    || column.dataIndex == 'F14'
                    || column.dataIndex == 'F15'
                    || column.dataIndex == 'F16'
                    || column.dataIndex == 'F17'
                    || column.dataIndex == 'F18'
                    || column.dataIndex == 'F19'
                    || column.dataIndex == 'F20'
                    || column.dataIndex == 'F21'
                    || column.dataIndex == 'F22'
                    || column.dataIndex == 'F23'
                    || column.dataIndex == 'F24'
                    || column.dataIndex == 'F25'
                    || column.dataIndex == 'F26'
                    || column.dataIndex == 'F29'
                    || column.dataIndex == 'F30'
                    || column.dataIndex == 'F31'
                    || column.dataIndex == 'F33'
                    || column.dataIndex == 'F34'
                    || column.dataIndex == 'F35'
                    || column.dataIndex == 'F36'
                    || column.dataIndex == 'F37'
                    || column.dataIndex == 'F38'
                    || column.dataIndex == 'F39'
                    || column.dataIndex == 'F40'
                    || column.dataIndex == 'F41'
                    || column.dataIndex == 'F43'
                    || column.dataIndex == 'F44'
                    || column.dataIndex == 'F45'
                    || column.dataIndex == 'F46'
                    || column.dataIndex == 'F47'
                    || column.dataIndex == 'F48'
                    || column.dataIndex == 'F49'
                    || column.dataIndex == 'F50'
                    || column.dataIndex == 'F51'
                    || column.dataIndex == 'F52'
                    || column.dataIndex == 'F53'
                    || column.dataIndex == 'F54'
                    || column.dataIndex == 'F55'
                ) {
                    column.setHidden(true);
                }
            });
            T1Grid.resumeLayouts(true);
        }

    }
    MenuLinkSet();

});
/*
F1-盤存單位 F2-藥材代碼 F3-廠商代碼
F4-藥材名稱 F61-英文品名 F5-藥材單位
F6-單價 F7-優惠後單價 F8-上月單價 F9-上月優惠後單價
F10-包裝量 F11-上月結存 F12_1-進貨入庫 F12_2-撥發入庫
F13-撥發出庫 F14-調撥入庫 F15-調撥出庫 F16-調帳入庫
F17-調帳出庫 F56-退料入庫 F57-退料出庫
F20-退貨量 F21-報廢量 F22-換貨入庫 F23-換貨出庫
F24-軍方消耗 F25-民眾消耗 F26-本月總消耗
F27-應有結存 F28-盤存量 F29-本月結存
F30-上月寄庫藥品買斷結存
F31-本月寄庫藥品買斷結存
F32-差異量 F33-結存金額 F34-優惠後結存金額
F35-差異金額 F36-優惠後差異金額 F37-藥材類別
F38-是否合約 F39-買斷寄庫 F40-是否戰備
F41-戰備存量 F42-期末比 F43-贈品數量
F44-上月戰備量 F45-上月是否戰備 F46-不含戰備上月結存
F47-單價差額 F48-不含戰備上月結存價差 F49-戰備本月價差
F50-不含戰備本月結存 F51-不含戰備本月應有結存量
F52-不含戰備本月盤存量 F53-不含贈品本月進貨
F54-單位申請基準量 F55-下月預計申請量
F58-本月單價5000元以上總消耗
F59-本月單價未滿5000元總消耗
F60-合約案號

------------
F1-上月結存 F2-本月進貨 F3-本月退貨 F4-本月贈品
F5-消耗金額 F6-軍方消耗 F7-民眾消耗 F8-退料金額
F9-調撥金額 F10-換貨金額 F11-報廢金額 F12-應有結存
F13-盤點結存 F14-本月結存 F24-盤盈金額 F25-盤虧金額
F26-合計盤盈虧總金額 F15-買斷結存
F16-上月寄庫藥品買斷結存
F17-本月寄庫藥品買斷結存
F18-戰備金額 F19-寄庫結存 F20-不含戰備上月結存價差
F21-戰備本月價差 F22-折讓金額

F28-撥發入庫金額 F29-撥發出庫金額

-------------
*/