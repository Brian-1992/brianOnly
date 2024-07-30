Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);


Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    
    var viewModel = Ext.create('WEBAPP.store.FA0060VM');
    var preym = '';
    var defaultColumns = [{
        xtype: 'rownumberer',
        width: 30,
        align: 'Center',
        labelAlign: 'Center'
    },
    {
        text: "歸戶",
        dataIndex: 'GRP_NAME',
        width: 80
    }, {
        text: "單位代碼",
        dataIndex: 'INID',
        width: 100
    }, {
        text: "單位名稱",
        dataIndex: 'INID_NAME',
        width: 150
    }];
    var T1GetExcel = '../../../api/FA0060/T1Excel';
    var T2GetExcel = '../../../api/FA0060/T2Excel';
    var T3GetExcel = '../../../api/FA0060/T3Excel';

    //#region store, combo
    var T1Store = viewModel.getStore('GetData');
    var T11Store = viewModel.getStore('GetData');
    function T1Load(clearMsg) {

        if (clearMsg) {
            msglabel('');
        }

        getDefaultColumns();

        Ext.Ajax.request({
            url: '/api/FA0060/Columns',
            method: reqVal_p,
            params: {
                data_ym: T1Query.getForm().findField('DATA_YM').getValue(),
                grp_no: T1Query.getForm().findField('GRP_NO').getValue(),
                inid: T1Query.getForm().findField('INID').getValue(),
                rate: T1Query.getForm().findField('RATE').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {

                    if (data.etts.length > 0) {
                        var columns = data.etts;
                        setT1GridColumns(columns);
                        console.log("column done");
                    }

                    T1Store.removeAll();

                    T1Store.getProxy().setExtraParam("data_ym", T1Query.getForm().findField('DATA_YM').getValue());
                    T1Store.getProxy().setExtraParam("grp_no", T1Query.getForm().findField('GRP_NO').getValue());
                    T1Store.getProxy().setExtraParam("inid", T1Query.getForm().findField('INID').getValue());
                    T1Store.getProxy().setExtraParam("rate", T1Query.getForm().findField('RATE').getValue());
                    T1Tool.moveFirst();
                    console.log("t1load");
                }
            },
            failure: function (response, options) {

            }
        });

    }

    var T2Store = viewModel.getStore('Details');
    function T2Load(clearMsg) {

        if (clearMsg) {
            msglabel('');
        }

        if (Number(T2Query.getForm().findField('DATA_YM_START').rawValue) < 10905 ||
            Number(T2Query.getForm().findField('DATA_YM_END').rawValue) < 10905) {
            Ext.Msg.alert('提醒', '統計年月必須大於等於<span style="color:red">10905</span>');
            return;
        }

        T2Store.getProxy().setExtraParam("data_ym_start", T2Query.getForm().findField('DATA_YM_START').rawValue);
        T2Store.getProxy().setExtraParam("data_ym_end", T2Query.getForm().findField('DATA_YM_END').rawValue);
        T2Store.getProxy().setExtraParam("grp_no", T2Query.getForm().findField('GRP_NO').getValue());
        T2Store.getProxy().setExtraParam("inid", T2Query.getForm().findField('INID').getValue());
        T2Store.getProxy().setExtraParam("mmcode", T2Query.getForm().findField('MMCODE').getValue());
        T2Tool.moveFirst();
    }

    var T3Store = viewModel.getStore('GetT3Data');
    function T3Load(clearMsg) {

        if (clearMsg) {
            msglabel('');
        }

        getT3DefaultColumns();

        Ext.Ajax.request({
            url: '/api/FA0060/GetT3Columns',
            method: reqVal_p,
            params: {
                year: T3Query.getForm().findField('YEAR').getValue(),
                q: T3Query.getForm().findField('Q').getValue(),
                grp_no: T3Query.getForm().findField('GRP_NO').getValue(),
                inid: T3Query.getForm().findField('INID').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {

                    if (data.etts.length > 0) {
                        var columns = data.etts;
                        setT3GridColumns(columns);
                    }

                    T3Store.removeAll();

                    T3Store.getProxy().setExtraParam("year", T3Query.getForm().findField('YEAR').getValue());
                    T3Store.getProxy().setExtraParam("q", T3Query.getForm().findField('Q').getValue());
                    T3Store.getProxy().setExtraParam("grp_no", T3Query.getForm().findField('GRP_NO').getValue());
                    T3Store.getProxy().setExtraParam("inid", T3Query.getForm().findField('INID').getValue());
                    
                    T3Tool.moveFirst();
                }
            },
            failure: function (response, options) {

            }
        });

    }

    var inidCombo = Ext.create('WEBAPP.form.InidWhCombo', {
        name: 'INID',
        width: 160,
        fieldLabel: '責任中心',
        allowBlank: true,
        labelWidth: 60,

        //限制一次最多顯示10筆
        limit: 10,
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            
            return {
                //WH_NO: tmpArray[0]
                queryString: T1Query.getForm().findField('INID').getValue(),
                p1: T1Query.getForm().findField('GRP_NO').getValue(),
            };
        },
        //指定查詢的Controller路徑
        queryUrl: '/api/FA0060/Inids',
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));


                var f = T1Query.getForm();
                if (r.get('INID') !== '') {
                    f.findField("INID").setValue(r.get('INID'));
                    f.findField("INID_NAME").setValue(r.get('INID_NAME'));
                }
            },
            change: function (field, newValue, oldValue) {
                
                if (newValue == '' || newValue == null) {
                    T1Query.getForm().findField("INID_NAME").setValue('');
                }
            }
        }
    });
    var inidCombo1 = Ext.create('WEBAPP.form.InidWhCombo', {
        name: 'INID',
        width: 160,
        fieldLabel: '責任中心',
        allowBlank: true,
        labelWidth: 60,

        //限制一次最多顯示10筆
        limit: 10,
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            return {
                //WH_NO: tmpArray[0]
                queryString: T2Query.getForm().findField('INID').getValue(),
                p1: T2Query.getForm().findField('GRP_NO').getValue(),
            };
        },
        //指定查詢的Controller路徑
        queryUrl: '/api/FA0060/Inids',
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));


                var f = T2Query.getForm();
                if (r.get('INID') !== '') {
                    f.findField("INID").setValue(r.get('INID'));
                    f.findField("INID_NAME").setValue(r.get('INID_NAME'));
                }
            },
            change: function (field, newValue, oldValue) {
                
                if (newValue == '' || newValue == null) {
                    T2Query.getForm().findField("INID_NAME").setValue('');
                }
            }
        }
    });
    var inidCombo2 = Ext.create('WEBAPP.form.InidWhCombo', {
        name: 'INID',
        width: 160,
        fieldLabel: '責任中心',
        allowBlank: true,
        labelWidth: 60,

        //限制一次最多顯示10筆
        limit: 10,
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            return {
                //WH_NO: tmpArray[0]
                queryString: T3Query.getForm().findField('INID').getValue(),
                p1: T3Query.getForm().findField('GRP_NO').getValue(),
            };
        },
        //指定查詢的Controller路徑
        queryUrl: '/api/FA0060/Inids',
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));


                var f = T3Query.getForm();
                if (r.get('INID') !== '') {
                    f.findField("INID").setValue(r.get('INID'));
                    f.findField("INID_NAME").setValue(r.get('INID_NAME'));
                }
            },
            change: function (field, newValue, oldValue) {

                if (newValue == '' || newValue == null) {
                    T3Query.getForm().findField("INID_NAME").setValue('');
                }
            }
        }
    });

    var urinidgrpCombo = Ext.create('WEBAPP.form.UrInidGrpCombo', {
        name: 'GRP_NO',
        width: 180,
        fieldLabel: '歸戶',
        allowBlank: true,
        labelWidth: 60,

        //限制一次最多顯示10筆
        limit: 10,
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            return {
                //WH_NO: tmpArray[0]
                queryString: T1Query.getForm().findField('GRP_NO').getValue()
            };
        },
        //指定查詢的Controller路徑
        queryUrl: '/api/FA0060/Grpnos',
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                T1Query.getForm().findField('INID').setValue('');
                T1Query.getForm().findField('INID_NAME').setValue('');

                var f = T1Query.getForm();
                if (r.get('GRP_NO') !== '') {
                    f.findField("GRP_NO").setValue(r.get('GRP_NO'));
                    f.findField("GRP_NAME").setValue(r.get('GRP_NAME'));
                }
            },
            change: function (field, newValue, oldValue) {
                

                if (newValue == '' || newValue == null) {
                    T1Query.getForm().findField("GRP_NAME").setValue('');
                    T1Query.getForm().findField('INID').setValue('');
                    T1Query.getForm().findField('INID_NAME').setValue('');
                }
            }
        }
    });
    var urinidgrpCombo1 = Ext.create('WEBAPP.form.UrInidGrpCombo', {
        name: 'GRP_NO',
        width: 180,
        fieldLabel: '歸戶',
        allowBlank: true,
        labelWidth: 60,

        //限制一次最多顯示10筆
        limit: 10,
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            return {
                //WH_NO: tmpArray[0]
                queryString: T2Query.getForm().findField('GRP_NO').getValue()
            };
        },
        //指定查詢的Controller路徑
        queryUrl: '/api/FA0060/Grpnos',
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                T2Query.getForm().findField('INID').setValue('');
                T2Query.getForm().findField('INID_NAME').setValue('');

                var f = T2Query.getForm();
                if (r.get('GRP_NO') !== '') {
                    f.findField("GRP_NO").setValue(r.get('GRP_NO'));
                    f.findField("GRP_NAME").setValue(r.get('GRP_NAME'));
                }
            },
            change: function (field, newValue, oldValue) {

                
                if (newValue == '' || newValue == null) {
                    T2Query.getForm().findField("GRP_NAME").setValue('');
                    T2Query.getForm().findField('INID').setValue('');
                    T2Query.getForm().findField('INID_NAME').setValue('');
                }
            }
        }
    });
    var urinidgrpCombo2 = Ext.create('WEBAPP.form.UrInidGrpCombo', {
        name: 'GRP_NO',
        width: 180,
        fieldLabel: '歸戶',
        allowBlank: true,
        labelWidth: 60,

        //限制一次最多顯示10筆
        limit: 10,
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            return {
                //WH_NO: tmpArray[0]
                queryString: T3Query.getForm().findField('GRP_NO').getValue()
            };
        },
        //指定查詢的Controller路徑
        queryUrl: '/api/FA0060/Grpnos',
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                T3Query.getForm().findField('INID').setValue('');
                T3Query.getForm().findField('INID_NAME').setValue('');

                var f = T3Query.getForm();
                if (r.get('GRP_NO') !== '') {
                    f.findField("GRP_NO").setValue(r.get('GRP_NO'));
                    f.findField("GRP_NAME").setValue(r.get('GRP_NAME'));
                }
            },
            change: function (field, newValue, oldValue) {


                if (newValue == '' || newValue == null) {
                    T3Query.getForm().findField("GRP_NAME").setValue('');
                    T3Query.getForm().findField('INID').setValue('');
                    T3Query.getForm().findField('INID_NAME').setValue('');
                }
            }
        }
    });

    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        allowBlank: true,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/FA0060/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T2Query.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                }
            }
        }
    });

    var yearStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
    });
    // 
    var qStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
            { "VALUE": "1", "TEXT": "Q1" },
            { "VALUE": "2", "TEXT": "Q2" },
            { "VALUE": "3", "TEXT": "Q3" },
            { "VALUE": "4", "TEXT": "Q4" }
        ]
    });
    //#endregion

    //#region tool
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export1', text: '匯出', handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: preym + '-不計價衛材分析表' + '.xls' });
                    p.push({ name: 'grp_no', value: T1Query.getForm().findField('GRP_NO').getValue() });
                    p.push({ name: 'inid', value: T1Query.getForm().findField('INID').getValue() });
                    p.push({ name: 'rate', value: T1Query.getForm().findField('RATE').getValue() });
                    p.push({ name: 'data_ym', value: T1Query.getForm().findField('DATA_YM').getValue() });
                    PostForm(T1GetExcel, p);
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
                itemId: 'export2', text: '匯出', handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '不計價衛材明細資料' + '.xls' });
                    p.push({ name: 'grp_no', value: T2Query.getForm().findField('GRP_NO').getValue() });
                    p.push({ name: 'inid', value: T2Query.getForm().findField('INID').getValue() });
                    p.push({ name: 'mmcode', value: T2Query.getForm().findField('MMCODE').getValue() });
                    p.push({ name: 'data_ym_start', value: T2Query.getForm().findField('DATA_YM_START').rawValue });
                    p.push({ name: 'data_ym_end', value: T2Query.getForm().findField('DATA_YM_END').rawValue });
                    PostForm(T2GetExcel, p);
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
                itemId: 'export1', text: '匯出', handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: preym + '-不計價衛材季度資料表' + '.xls' });
                    p.push({ name: 'grp_no', value: T3Query.getForm().findField('GRP_NO').getValue() });
                    p.push({ name: 'inid', value: T3Query.getForm().findField('INID').getValue() });
                    p.push({ name: 'year', value: T3Query.getForm().findField('YEAR').getValue() });
                    p.push({ name: 'q', value: T3Query.getForm().findField('Q').getValue() });
                    PostForm(T3GetExcel, p);
                }
            }
        ]
    });
    //#endregion

    //#region query
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 80,
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'displayfield',
                    fieldLabel: '統計年月',
                    name: 'DATA_YM'
                },
                urinidgrpCombo,
                {
                    xtype: 'displayfield',
                    name: 'GRP_NAME',
                    labelAlign: 'left',
                    labelSeparator: '',
                    width: 100,
                    padding: '0 0 0 10'
                },
                inidCombo,
                {
                    xtype: 'displayfield',
                    name: 'INID_NAME',
                    labelAlign: 'left',
                    labelSeparator: '',
                    width: 100,
                    padding: '0 0 0 10'
                },
                //{
                //    xtype: 'monthfield',
                //    fieldLabel: '月份',
                //    name: 'P1',
                //    id: 'P1',
                //    enforceMaxLength: true,
                //    padding: '0 4 0 4',
                //    allowBlank: true, // 欄位是否為必填
                //    format: 'Xm'
                //},
                {
                    xtype: 'button',
                    text: '轉入上月資料',
                    handler: function () {
                        msglabel("");
                        transfer('1');
                        // viewport.down('#form').setCollapsed("true");

                    }
                },

            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'numberfield',
                    fieldLabel: '目標值：去年同季',
                    name: 'RATE',
                    labelWidth: 100,
                    width: 140,
                    labelSeparator: '',
                    submitValue: true,
                    allowBlank: false,
                    fieldCls: 'required',
                    hideTrigger: true,
                    minValue: 1,
                    maxValue: 100,
                    value: 95,
                    labelAlign: 'right',
                },
                {
                    xtype: 'displayfield',
                    labelAlign: 'left',
                    labelSeparator: '',
                    value: '%',
                    width: 10
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        msglabel("");
                        T1Load();
                        // viewport.down('#form').setCollapsed("true");

                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        T1Query.getForm().findField('DATA_YM').setValue(preym);
                    }
                }, {
                    xtype: 'button',
                    text: '歸戶資料維護',
                    handler: function () {
                        popWinFormAA0041('011');
                    }
                }
            ]
        }]
    });

    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 80,
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP21',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'monthfield',
                    fieldLabel: '統計年月',
                    name: 'DATA_YM_START',
                    width: 180,
                },
                {
                    xtype: 'monthfield',
                    fieldLabel: '至',
                    name: 'DATA_YM_END',
                    labelSeparator: '',
                    width: 120,
                    labelWidth: 20,
                },
                urinidgrpCombo1,
                {
                    xtype: 'displayfield',
                    name: 'GRP_NAME',
                    labelAlign: 'left',
                    labelSeparator: '',
                    width: 100,
                    padding: '0 0 0 10'
                },
                inidCombo1,
                {
                    xtype: 'displayfield',
                    name: 'INID_NAME',
                    labelAlign: 'left',
                    labelSeparator: '',
                    width: 100,
                    padding: '0 0 0 10'
                },

            ]
        }, {
            xtype: 'panel',
            id: 'PanelP22',
            border: false,
            layout: 'hbox',
            items: [
                mmCodeCombo,
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        msglabel("");
                        T2Load();
                        // viewport.down('#form').setCollapsed("true");

                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        T2Query.getForm().findField('DATA_YM_START').setValue(preym);
                        T2Query.getForm().findField('DATA_YM_END').setValue(preym);
                    }
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '衛材 1090504 上線',
                    labelWidth: 150,
                    labelSeparator: '',
                    labelStyle: 'color: gray;',
                }
            ]
        }]
    });

    var T3Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 80,
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '年分',
                    name: 'YEAR',
                    store: yearStore,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    allowBlank: false, // 欄位為必填
                    typeAhead: true,
                    forceSelection: true,
                    queryMode: 'local',
                    triggerAction: 'all',
                    fieldCls: 'required',
                    width: 130,
                    labelWidth: 60,
                },
                {

                    xtype: 'combo',
                    fieldLabel: '季度',
                    name: 'Q',
                    enforceMaxLength: true,
                    width: 100,
                    padding: '0 4 0 4',
                    store: qStore,
                    fieldCls: 'required',
                    allowBlank: false, // 欄位為必填
                    typeAhead: true,
                    forceSelection: true,
                    queryMode: 'local',
                    triggerAction: 'all',
                    fieldCls: 'required',
                    value: '02',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    labelWidth: 50,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '衛材 1090504 上線',
                    labelWidth: 100,
                    labelSeparator: '',
                    labelStyle: 'color: gray;',
                },
                //{
                //    xtype: 'textfield',
                //    fieldLabel: '季度',
                //    name: 'Q',
                //    fieldCls: 'required',
                //    width: 120
                //    //allowBlank: false
                //},
                urinidgrpCombo2,
                {
                    xtype: 'displayfield',
                    name: 'GRP_NAME',
                    labelAlign: 'left',
                    labelSeparator: '',
                    width: 100,
                    padding: '0 0 0 10'
                },
                inidCombo2,
                {
                    xtype: 'displayfield',
                    name: 'INID_NAME',
                    labelAlign: 'left',
                    labelSeparator: '',
                    width: 100,
                    padding: '0 0 0 10'
                },
                //{
                //    xtype: 'monthfield',
                //    fieldLabel: '月份',
                //    name: 'P1',
                //    id: 'P1',
                //    enforceMaxLength: true,
                //    padding: '0 4 0 4',
                //    allowBlank: true, // 欄位是否為必填
                //    format: 'Xm'
                //},
                {
                    xtype: 'button',
                    text: '轉入資料',
                    handler: function () {
                        msglabel("");
                        transferByYmWindow.show();
                        //transfer('3');
                        // viewport.down('#form').setCollapsed("true");

                    }
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        msglabel("");
                        T3Load();
                        // viewport.down('#form').setCollapsed("true");

                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        //T3Query.getForm().findField('YEAR').setValue(preym);
                    }
                }, {
                    xtype: 'button',
                    text: '歸戶資料維護',
                    handler: function () {
                        popWinFormAA0041('011');
                    }
                }
            ]
        }]
    });
    //#endregion

    //#region grid
    // T1 GRID查詢結果
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                /*dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',*/
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [{
            xtype: 'rownumberer',
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        },
        {
            text: "歸戶",
            dataIndex: 'GRP_NAME',
            width: 80
        }, {
            text: "單位代碼",
            dataIndex: 'INID',
            width: 100
        }, {
            text: "單位名稱",
            dataIndex: 'INID_NAME',
            width: 150
        }]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: T2Name,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                /*dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',*/
                items: [T2Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        columns: [{
            xtype: 'rownumberer',
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        },
        {
            text: "年月",
            dataIndex: 'DATA_YM',
            width: 80
        }, {
            text: "歸戶",
            dataIndex: 'GRP_NAME',
            width: 120
        }, {
            text: "單位代碼",
            dataIndex: 'INID',
            width: 100
        }, {
            text: "單位名稱",
            dataIndex: 'INID_NAME',
            width: 150
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 150
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150,
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 100,
        }, {
            text: "庫存單價",
            dataIndex: 'AVG_PRICE',
            width: 100,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "消耗數量",
            dataIndex: 'USE_QTY',
            width: 100,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "消耗金額",
            dataIndex: 'USE_AMOUNT',
            width: 100,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "週轉率",
            dataIndex: 'TUNROVER',
            width: 100,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }, {
            text: "庫備否",
            dataIndex: 'MSTOREID',
            width: 80,
        }]
    });

    var T3Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T3',
        dockedItems: [
            {
                /*dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',*/
                items: [T3Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Tool]
            }
        ],
        columns: [{
            xtype: 'rownumberer',
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        },
        {
            text: "歸戶",
            dataIndex: 'GRP_NAME',
            width: 80
        }, {
            text: "單位代碼",
            dataIndex: 'INID',
            width: 100
        }, {
            text: "單位名稱",
            dataIndex: 'INID_NAME',
            width: 150
        }]
    });
    //#endregion

    //#region function
    function getPreym() {
        Ext.Ajax.request({
            url: '/api/FA0060/GetPreym',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    preym = data.msg;
                    T1Query.getForm().findField('DATA_YM').setValue(data.msg);
                    T2Query.getForm().findField('DATA_YM_START').setValue(data.msg);
                    T2Query.getForm().findField('DATA_YM_END').setValue(data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function getDefaultColumns() {


        T1Grid.reconfigure(null, defaultColumns);
    }
    function setT1GridColumns(columns) {

        for (var i = 0; i < columns.length; i++) {

            var index = columns[i].TEXT.indexOf('達標否');

            var column = null;
            if (index < 0) {
                column = Ext.create('Ext.grid.column.Column', {
                    text: columns[i].TEXT,
                    dataIndex: columns[i].DATAINDEX,
                    width: i == 0 ? 150 : 120,
                    align: 'right',
                    style: 'text-align:left',
                });
            } else {
                column = Ext.create('Ext.grid.column.Column', {
                    text: columns[i].TEXT,
                    dataIndex: columns[i].DATAINDEX,
                    width: i == 0 ? 150 : 120,
                });
            }

            T1Grid.headerCt.insert(T1Grid.columns.length, column);
            //T1Grid.getView().refresh();
            T1Grid.columns.push(column);
        }
        T1Grid.getView().refresh();
    }

    function getT3DefaultColumns() {


        T3Grid.reconfigure(null, defaultColumns);
    }
    function setT3GridColumns(columns) {

        for (var i = 0; i < columns.length; i++) {

            var index = columns[i].TEXT.indexOf('達標否');

            var column = null;
            if (index < 0) {
                column = Ext.create('Ext.grid.column.Column', {
                    text: columns[i].TEXT,
                    dataIndex: columns[i].DATAINDEX,
                    width: i == 0 ? 150 : 120,
                    align: 'right',
                    style: 'text-align:left',
                });
            } else {
                column = Ext.create('Ext.grid.column.Column', {
                    text: columns[i].TEXT,
                    dataIndex: columns[i].DATAINDEX,
                    width: i == 0 ? 150 : 120,
                });
            }

            T3Grid.headerCt.insert(T3Grid.columns.length, column);
            //T1Grid.getView().refresh();
            T3Grid.columns.push(column);
        }
        T3Grid.getView().refresh();
    }

    function transfer(tab) {
        Ext.Ajax.request({
            url: '/api/FA0060/Transfer',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('資料轉入完成');
                    if (tab == '1') {
                        T1Load(false);
                    }
                    if (tab == '2') {
                        T2Load(false);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function getYearCombo() {
        Ext.Ajax.request({
            url: '/api/FA0060/GetYearCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    for (var i = 0; i < data.etts.length; i++) {
                        yearStore.add({ VALUE: data.etts[i].VALUE, TEXT: data.etts[i].TEXT});
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    //#endregion

    //#region window
    var T31Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    layout: 'vbox',
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '轉檔年月',
                            name: 'YM',
                        },
                    ]
                },

            ]
        }]
    });

    var transferByYmWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T31Form],
        //width: "900px",
        //height: windowHeight,
        resizable: false,
        draggable: false,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        //y: 0,
        title: "轉檔",
        //listeners: {
        //    close: function (panel, eOpts) {
        //        myMask.hide();
        //    }
        //}
        buttons: [
            {
                text: '確定',
                id: 'transferT3',
                handler: function () {
                    if (T31Form.getForm().findField('YM').getValue() == null || T31Form.getForm().findField('YM').getValue() == undefined) {
                        Ext.Msg.alert('提醒', '請輸入轉檔月份');
                        return;
                    }
                    
                    url = '/api/FA0060/TransferByYm';

                    var windowMask = new Ext.LoadMask(transferByYmWindow, { msg: '處理中...' });

                    myMask.show();
                    windowMask.show();
                    Ext.Ajax.request({
                        url: url,
                        method: reqVal_p,
                        params: {
                            ym: T31Form.getForm().findField('YM').getValue()
                        },
                        timeout: 300000,
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('轉檔成功，請重新查詢');
                                myMask.hide();
                                windowMask.hide();
                            } else {
                                Ext.Msg.alert('失敗', '發生例外錯誤');
                                myMask.hide();
                                windowMask.hide();
                            }
                            transferByYmWindow.hide();
                        },
                        failure: function (response, options) {
                            myMask.hide();
                            windowMask.hide();
                            transferByYmWindow.hide();
                        }
                    });
                }
            },
            {
                text: '取消',
                handler: function () {
                    T31Form.getForm().reset();
                    transferByYmWindow.hide();
                }
            }
            //{
            //text: '關閉',
            //handler: function () {
            //    filterWindow.hide();
            //}
            //}
        ],
        listeners: {
            show: function (self, eOpts) {

                transferByYmWindow.center();
            }
        }
    });
    transferByYmWindow.hide();
    //#endregion


    //#region 定義TAB內容
    var TATabs = Ext.widget('tabpanel', {
        layout: 'fit',
        plain: true,
        border: false,
        resizeTabs: true,       //改變tab尺寸       
        enableTabScroll: true,  //是否允許Tab溢出時可以滾動
        defaults: {
            // autoScroll: true,
            closabel: false,    //tab是否可關閉
            padding: 0,
            split: true
        },
        items: [{
            itemId: 't1GridTab',
            title: '分析表',
            layout: 'border',
            padding: 0,
            split: true,
            region: 'center',
            layout: 'fit',
            collapsible: false,
            border: false,
            items: [T1Grid]
        }, {
            itemId: 't2GridTab',
            title: '明細資料',
            layout: 'border',
            padding: 0,
            split: true,
            region: 'center',
            layout: 'fit',
            collapsible: false,
            border: false,
            items: [T2Grid]
            }, {
                itemId: 't3GridTab',
                title: '季度資料',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T3Grid]
            }]
    });

    var callableWin = null;
    popWinFormAA0041 = function (strParam) {
        var strUrl = "/Form/Index/AA0041?parBtnVis=" + strParam;
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
                        T1Load();
                        T2Load();
                    }
                }]
            });
            callableWin = GetPopWin(viewport, popform, '歸戶資料維護', viewport.width - 10, viewport.height - 10);
        }
        callableWin.show();
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
            itemId: 't1Form',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [TATabs]
        }
        ]
    });
    //#endregion
    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    getPreym();
    getYearCombo();

});
