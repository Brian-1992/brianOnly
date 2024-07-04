Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    
    var st_matclasssub = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    
    var st_e_sourcecode = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var st_m_contid = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    
    var st_orderkind = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: '/api/AA0190/GetMatClassSubCombo',
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
            url: '/api/AA0190/GetESourceCodeCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var e_sourcecode = data.etts;
                    if (e_sourcecode.length > 0) {
                        for (var i = 0; i < e_sourcecode.length; i++) {
                            st_e_sourcecode.add({ VALUE: e_sourcecode[i].VALUE, TEXT: e_sourcecode[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: '/api/AA0190/GetMContidCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var m_contid = data.etts;
                    if (m_contid.length > 0) {
                        for (var i = 0; i < m_contid.length; i++) {
                            st_m_contid.add({ VALUE: m_contid[i].VALUE, TEXT: m_contid[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        
        Ext.Ajax.request({
            url: '/api/AA0190/GetOrderkindCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var orderkind = data.etts;
                    if (orderkind.length > 0) {
                        for (var i = 0; i < orderkind.length; i++) {
                            st_orderkind.add({ VALUE: orderkind[i].VALUE, TEXT: orderkind[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

    }
    setComboData();
    
    var T1QuerymmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P0',
        name: 'P0',
        fieldLabel: '藥材代碼',
        labelWidth: 100,
        width: 300,
        emptyText: '全部',
        //allowBlank: true,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AA0190/GetMMCodeCombo',
        //查詢完會回傳的欄位
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
        }
    });

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        autoScroll: true,
        layout: 'vbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 100,
            labelAlign: 'right',
            style: 'text-align:center'
        },
        border: false,
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                items: [{
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        T1QuerymmCodeCombo, //P0
                        {
                            xtype: 'combo',
                            fieldLabel: '藥材類別',
                            name: 'P1',
                            id: 'P1',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_matclasssub,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        }, {
                            xtype: 'tbspacer',
                            width: 10
                        }, {
                            xtype: 'button',
                            text: '查詢',
                            id: 'T1btn1',
                            handler: function () {
                                if (T1Query.getForm().isValid()) {
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
                        }
                    ]
                },
                    {
                        xtype: 'panel',
                        //id: 'PanelP1',
                        border: false,
                        layout: 'hbox',
                        items: [{
                            xtype: 'combo',
                            fieldLabel: '是否合約',
                            name: 'P2',
                            id: 'P2',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_m_contid,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '買斷寄庫',
                            name: 'P3',
                            id: 'P3',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_e_sourcecode,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },]
                    }, {
                    xtype: 'panel',
                    id: 'PanelP2',
                    border: false,
                    layout: 'hbox',
                        items: [
                            {
                                xtype: 'combo',
                                fieldLabel: '採購類別',
                                name: 'P4',
                                id: 'P4',
                                labelWidth: 100,
                                width: 300,
                                emptyText: '全部',
                                store: st_orderkind,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            }, {
                                xtype: 'label',
                                text: '不含',
                                width: 100,
                                style: 'text-align:right',
                                
                            },
                            {
                                xtype: 'panel',
                                id: 'PanelP21',
                                border: false,
                                layout: 'hbox',
                                items: [
                                    {
                                        xtype: 'checkboxfield',
                                        boxLabel: '刪除品項',
                                        name: 'P5',
                                        id: 'P5',
                                        style: 'margin:0px 3px 0px 3px;',
                                        labelWidth: 80,
                                        width: 90,
                                        inputValue: '1',
                                        checked: true
                                    },
                                    {
                                        xtype: 'checkboxfield',
                                        boxLabel: '特殊藥品',
                                        name: 'P6',
                                        id: 'P6',
                                        style: 'margin:0px 3px 0px 3px;',
                                        labelWidth: 80,
                                        width: 90,
                                        inputValue: '1',
                                        checked: true
                                    },
                                    {
                                        xtype: 'checkboxfield',
                                        boxLabel: '急救品項',
                                        name: 'P7',
                                        id: 'P7',
                                        style: 'margin:0px 3px 0px 3px;',
                                        labelWidth: 80,
                                        width: 90,
                                        inputValue: '1',
                                        checked: true
                                    },
                                    {
                                        xtype: 'checkboxfield',
                                        boxLabel: '管制藥品',
                                        name: 'P8',
                                        id: 'P8',
                                        style: 'margin:0px 3px 0px 3px;',
                                        labelWidth: 80,
                                        width: 90,
                                        inputValue: '1',
                                        checked: true
                                    },
                                    {
                                        xtype: 'checkboxfield',
                                        boxLabel: '未到貨品項',
                                        name: 'P9',
                                        id: 'P9',
                                        style: 'margin:0px 3px 0px 3px;',
                                        labelWidth: 80,
                                        width: 90,
                                        inputValue: '1',
                                        checked: true
                                    }
                                ]
                            },
                            {
                                xtype: 'tbspacer',
                                width: 10
                            }
                        ]
                    },
                    {
                        xtype: 'panel',
                        id: 'PanelP3',
                        border: false,
                        layout: 'hbox',
                        items: [
                            {
                                xtype: 'numberfield',
                                fieldLabel: '包裝進貨比',
                                name: 'P10',
                                id: 'P10',
                                labelWidth: 100,
                                width: 170,
                                allowDecimals: false, //是否允許輸入小數
                                decimalPrecision: 0,
                                maxLength: 3,
                                enforceMaxLength: true,
                                minValue: 0,
                                maxValue: 100
                            }, {
                                xtype: 'label',
                                text: '%以上進為(100:捨去,0:進位)'
                            },
                            {
                                xtype: 'tbspacer',
                                width: 30
                            }, {
                                xtype: 'checkboxfield',
                                style: 'margin:0px 5px 0px 5px;',
                                labelWidth: 100,
                                width: 110,
                                id: 'P11',
                                name: 'P11',
                                boxLabel: '建議量不為0'
                            }, {
                                xtype: 'checkboxfield',
                                style: 'margin:0px 5px 0px 5px;',
                                labelWidth: 120,
                                width: 130,
                                id: 'P12',
                                name: 'P12',
                                boxLabel: '建議量扣除未到貨'
                            },
                            {
                            xtype: 'tbspacer',
                            width: 10
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
            { name: 'F13', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 30, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F1', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0190/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
        },
        listeners: {
            load: function (store, options) {

            }
        }
    });

    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').checked ? 'Y' : 'N');
        T1Store.getProxy().setExtraParam("p6", T1Query.getForm().findField('P6').checked ? 'Y' : 'N');
        T1Store.getProxy().setExtraParam("p7", T1Query.getForm().findField('P7').checked ? 'Y' : 'N');
        T1Store.getProxy().setExtraParam("p8", T1Query.getForm().findField('P8').checked ? 'Y' : 'N');
        T1Store.getProxy().setExtraParam("p9", T1Query.getForm().findField('P9').checked ? 'Y' : 'N');
        T1Store.getProxy().setExtraParam("p10", T1Query.getForm().findField('P10').getValue());
        T1Store.getProxy().setExtraParam("p11", T1Query.getForm().findField('P11').checked ? 'Y' : 'N');
        T1Store.getProxy().setExtraParam("p12", T1Query.getForm().findField('P12').checked ? 'Y' : 'N');
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'apply', text: '轉為申購單', disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let name = '';
                        let mmcode = '';
                        let prqty = '';
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('F1') + '」<br>';
                            mmcode += item.get('F1') + ',';
                            prqty += item.get('F6') + ',';
                        })
                        Ext.MessageBox.confirm('轉為申購單', '是否確定轉為申購單？', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0190/Apply',
                                    method: reqVal_p,
                                    params: {
                                        MMCODE: mmcode,
                                        PRQTY: prqty
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            msglabel('訊息區:轉為申購單成功');
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
            }
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
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
        selModel:
        {
            mode: 'MULTI',
            allowDeselect: true,
            checkOnly: false,
            injectCheckbox: 'first'
        },
        selType: 'checkboxmodel',
        columns: [
            { xtype: 'rownumberer' },
            { text: "藥材代碼", dataIndex: 'F1', width: 100 },
            { text: "藥材名稱", dataIndex: 'F2', width: 200 },
            { text: "最低安全存量", dataIndex: 'F3', width: 120, style: 'text-align:left', align: 'right' },
            { text: "正常存量", dataIndex: 'F4', width: 100, style: 'text-align:left', align: 'right' },
            { text: "現在存量", dataIndex: 'F5', width: 100, style: 'text-align:left', align: 'right' },
            { text: "建議訂購量", dataIndex: 'F6', width: 100, style: 'text-align:left', align: 'right' },
            { text: "戰備存量", dataIndex: 'F7', width: 100, style: 'text-align:left', align: 'right' },
            { text: "核可訂單編號", dataIndex: 'F8', width: 200  },
            { text: "訂購數量", dataIndex: 'F9', width: 100, style: 'text-align:left', align: 'right'},
            { text: "藥材單位", dataIndex: 'F10', width: 80 },
            { text: "包裝量", dataIndex: 'F11', width: 100, style: 'text-align:left', align: 'right'},
            { text: "健保碼", dataIndex: 'F12', width: 160 },
            { text: "轉換量", dataIndex: 'F13', width: 100, style: 'text-align:left', align: 'right'},
            { header: "", flex: 1 }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1Grid.down('#apply').setDisabled(T1Rec === 0);
                
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
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();
});