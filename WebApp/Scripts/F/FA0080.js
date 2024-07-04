
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {

    var T1Name = "進出明細查詢";
    var UrlReport = '/Report/F/FA0080.aspx';
    var T1GetExcel = '/api/FA0080/Excel';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };

    var menuLink = Ext.getUrlParam('menuLink');

    var st_matclasssub = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var st_common = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var eRestrictCodeStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: '/api/FA0080/GetCursetym',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Query.getForm().findField('P0').setValue(data.msg);
                    T1Query.getForm().findField('P0').setRawValue(data.msg);
                    T1Query.getForm().findField('P1').setValue(data.msg);
                    T1Query.getForm().findField('P1').setRawValue(data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: '/api/FA0080/GetMatClassSubCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var matclasssubs = data.etts;
                    if (matclasssubs.length > 0) {
                        for (var i = 0; i < matclasssubs.length; i++) {
                            st_matclasssub.add({ VALUE: matclasssubs[i].VALUE, TEXT: matclasssubs[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: '/api/FA0080/GetCommonCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var commons = data.etts;
                    if (commons.length > 0) {
                        for (var i = 0; i < commons.length; i++) {
                            st_common.add({ VALUE: commons[i].VALUE, TEXT: commons[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        
        Ext.Ajax.request({
            url: '/api/FA0080/GetERestrictCodeCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            eRestrictCodeStore.add({ VALUE: tb_data[i].VALUE, TEXT: tb_data[i].TEXT, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var T1QueryWhNoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'P4',
        id: 'P4',
        fieldLabel: '庫房',
        labelAlign: 'right',
        fieldCls: 'required',
        allowBlank: false,
        labelWidth: 80,
        width: 290,
        limit: 10,//限制一次最多顯示10筆
        queryUrl: '/api/FA0080/GetWhnoCombo',//指定查詢的Controller路徑
        extraFields: ['WH_KIND', 'WH_GRADE'],//查詢完會回傳的欄位
        getDefaultParams: function () {//查詢時Controller固定會收到的參數
            return {
                menulink: menuLink
            };
        },
        listeners: {
            select: function (c, r, i, e) {
            }
        }
    });

    var T1QuerymmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P5',
        id: 'P5',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        fieldCls: 'required',
        allowBlank: true,
        labelWidth: 80,
        width: 290,
        limit: 10,
        //限制一次最多顯示10筆
        queryUrl: '/api/FA0080/GetMMCodeCombo',//指定查詢的Controller路徑
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],//查詢完會回傳的欄位
        getDefaultParams: function () {//查詢時Controller固定會收到的參數
            return {
                p2: T1Query.getForm().findField('P2').getValue() == null ? '' : T1Query.getForm().findField('P2').getValue(),
                p3: T1Query.getForm().findField('P3').getValue(),
                p4: T1Query.getForm().findField('P4').getValue(),
                p6: T1Query.getForm().findField('P6').getValue(),
                menulink: menuLink
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                T1Query.getForm().findField('MMNAME_C').setValue(r.data.MMNAME_C); //選取下拉項目時，顯示回傳值
            }
        }
    });
    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        layout: 'vbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 70,
            labelAlign: 'right',
            style: 'text-align:center'
        },
        border: false,
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                items: [
                    {
                        xtype: 'panel',
                        id: 'PanelP1',
                        border: false,
                        layout: 'hbox',
                        items: [
                            {
                                xtype: 'monthfield',
                                fieldLabel: '查詢月份',
                                name: 'P0',
                                id: 'P0',
                                labelWidth: 80,
                                width: 180,
                                fieldCls: 'required',
                                allowBlank: false
                            }, {
                                xtype: 'monthfield',
                                fieldLabel: '~',
                                name: 'P1',
                                id: 'P1',
                                labelSeparator: '',
                                enforceMaxLength: true,
                                labelWidth: 10,
                                width: 110,
                                fieldCls: 'required',
                                allowBlank: false
                            }, {
                                xtype: 'combo',
                                fieldLabel: '藥材類別',
                                name: 'P2',
                                id: 'P2',
                                labelWidth: 80,
                                width: 290,
                                hidden: true,
                                emptyText: '全部',
                                store: st_matclasssub,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            }, {
                                xtype: 'combo',
                                fieldLabel: '用品別',
                                name: 'P3',
                                id: 'P3',
                                labelWidth: 80,
                                width: 290,
                                hidden: true,
                                emptyText: '全部',
                                store: st_common,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            },
                            T1QueryWhNoCombo,
                            T1QuerymmCodeCombo,
                            {
                                xtype: 'combo',
                                fieldLabel: '管制級數',
                                name: 'P6',
                                id: 'P6',
                                labelWidth: 80,
                                width: 290,
                                hidden: true,
                                store: eRestrictCodeStore,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            }
                        ]
                    }, {
                        xtype: 'panel',
                        id: 'PanelP2',
                        border: false,
                        layout: 'hbox',
                        items: [
                            //{
                            //    xtype: 'checkbox',
                            //    name: 'P7',
                            //    width: 100,
                            //    boxLabel: '顯示沖帳項目',
                            //    inputValue: 'Y',
                            //    checked: false,
                            //    padding: '0 4 0 20'
                            //},
                            {
                                xtype: 'button',
                                text: '查詢',
                                id: 'T1btn1',
                                handler: function () {
                                    if (T1Query.getForm().findField("P0").getValue() == null || T1Query.getForm().findField("P0").getValue() == ""
                                        || T1Query.getForm().findField("P1").getValue() == null || T1Query.getForm().findField("P1").getValue() == ""
                                        || T1Query.getForm().findField("P4").getValue() == null || T1Query.getForm().findField("P4").getValue() == ""
                                        || T1Query.getForm().findField("P5").getValue() == null || T1Query.getForm().findField("P5").getValue() == ""
                                    ) {
                                        Ext.Msg.alert('提醒', '年月、庫房及藥材不可空白');
                                    }
                                    //else if (menuLink == "AB0147" && (T1Query.getForm().findField("P6").getValue() == null || T1Query.getForm().findField("P6").getValue() == ""))
                                    //{
                                    //    Ext.Msg.alert('提醒', '管制級數不可空白');
                                    //}
                                    else {
                                        T1Load();
                                    }
                                }
                            }, {
                                xtype: 'button',
                                text: '清除',
                                handler: function () {
                                    var f = this.up('form').getForm();
                                    f.reset();
                                    f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                                }
                            }, {
                                xtype: 'tbspacer',
                                width: 514
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '院內碼名稱',
                                name: 'MMNAME_C',
                                id: 'MMNAME_C',
                                labelWidth: 80,
                                width: 350,
                                style: 'text-align:left'
                            }
                        ]
                    }
                ]
            }

        ]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'F1', type: 'string' },
            { name: 'F2', type: 'string' },
            { name: 'F3', type: 'string' },
            { name: 'F4', type: 'string' },
            { name: 'F5', type: 'string' },
            { name: 'F6', type: 'string' },
            { name: 'F7', type: 'string' },
            { name: 'F8', type: 'string' },
            { name: 'F9', type: 'string' },
            { name: 'F10', type: 'string' },
            { name: 'F11', type: 'string' },
            { name: 'F12', type: 'string' },
            { name: 'F13', type: 'string' },
            { name: 'F14', type: 'string' },
            { name: 'F15', type: 'string' },
            { name: 'F16', type: 'string' },
            { name: 'F17', type: 'string' },
            { name: 'F18', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 30, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F1', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0080/AllM',
            //timeout: 9000000,
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
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    menulink: menuLink
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts, aaa) {
                if (successful) {
                    var dataCount = store.getCount();
                    if (dataCount > 0) {
                        T1Tool.down('#export').setDisabled(false);
                        T1Tool.down('#print').setDisabled(false);
                    }
                    else {
                        T1Tool.down('#export').setDisabled(true);
                        T1Tool.down('#print').setDisabled(true);
                    }
                }
                else {
                    if (menuLink == 'AB0147') {
                        Ext.Msg.alert('錯誤', '此院內碼非藥材或非管制藥，請重新查詢!'); 
                    } else {
                        Ext.Msg.alert('錯誤', '此院內碼非藥材，請重新查詢!'); 
                    }
                    
                }
            }
        }
    });

    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
        T1Tool.moveFirst();
    }


    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'print', text: '列印', disabled: true,
                handler: function () {
                    showReport();
                }
            },
            {
                itemId: 'export', text: '匯出', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue }); //SQL篩選條件
                            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue }); //SQL篩選條件
                            p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() }); //SQL篩選條件
                            p.push({ name: 'p5', value: T1Query.getForm().findField('P5').getValue() }); //SQL篩選條件
                            p.push({ name: 'p6', value: T1Query.getForm().findField('P6').getValue() }); //SQL篩選條件
                            p.push({ name: 'menulink', value: menuLink }); 
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            }

        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T1Store,
        plain: true,
        //loadingText: '處理中...',
        //loadMask: true,
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
            { text: "項次", dataIndex: 'F1', width: 60 },
            {
                text: "日期", dataIndex: 'F2', width: 90, 
            },
            { text: "進貨/撥發入", dataIndex: 'F3', width: 100, style: 'text-align:left', align: 'right' },
            { text: "撥發出", dataIndex: 'F4', width: 80, style: 'text-align:left', align: 'right' },
            { text: "調撥入", dataIndex: 'F5', width: 80, style: 'text-align:left', align: 'right' },
            { text: "調撥出", dataIndex: 'F6', width: 80, style: 'text-align:left', align: 'right' },
            { text: "調帳入", dataIndex: 'F7', width: 80, style: 'text-align:left', align: 'right' },
            { text: "調帳出", dataIndex: 'F8', width: 80, style: 'text-align:left', align: 'right' },
            { text: "退料入", dataIndex: 'F9', width: 80, style: 'text-align:left', align: 'right' },
            { text: "退料出", dataIndex: 'F10', width: 80, style: 'text-align:left', align: 'right' },
            { text: "退貨", dataIndex: 'F11', width: 80, style: 'text-align:left', align: 'right' },
            { text: "報廢", dataIndex: 'F12', width: 80, style: 'text-align:left', align: 'right' },
            { text: "換貨入", dataIndex: 'F13', width: 80, style: 'text-align:left', align: 'right' },
            { text: "換貨出", dataIndex: 'F14', width: 80, style: 'text-align:left', align: 'right' },
            { text: "消耗", dataIndex: 'F15', width: 90, style: 'text-align:left', align: 'right' },
            { text: "盤點差異", dataIndex: 'F16', width: 90, style: 'text-align:left', align: 'right' },
            { text: "結存", dataIndex: 'F17', width: 80, style: 'text-align:left', align: 'right' },
            { text: "異動單據", dataIndex: 'F18', width: 150, style: 'text-align:left'},
            { text: "上次異動人員", dataIndex: 'UPDATE_USER', width: 120,  },
            { text: "上次異動時間", dataIndex: 'UPDATE_TIME', width: 120,  },
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
        items: [
            {
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                split: true,
                items: [T1Grid]
            }
        ]
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + UrlReport
                + '?P0=' + T1Query.getForm().findField('P0').rawValue
                + '&P1=' + T1Query.getForm().findField('P1').rawValue
                + '&P4=' + T1Query.getForm().findField('P4').getValue()
                + '&P5=' + T1Query.getForm().findField('P5').getValue()
                + '&P6=' + T1Query.getForm().findField('P6').getValue()
                + '&MENULINK=' + menuLink
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


    function MenuLinkSet() {
        if (menuLink == "AB0147") {
            //Ext.getCmp('P6').show();
        }
    }

    MenuLinkSet();
});