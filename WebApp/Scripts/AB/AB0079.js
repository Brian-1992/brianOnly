Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var viewModel = Ext.create('WEBAPP.store.AB0079VM');   // 定義於/Scripts/app/store/AB0079VM.js
    var WHStore = viewModel.getStore('CD0007WH_NO');
    var T1Store = viewModel.getStore('ME_AB0079');        // 定義於/Scripts/app/store/AB0079VM.js

    var T1Set = ''; // 新增/修改/刪除
    var T1Get = '../../../api/AB0079/QueryD';      // ../../../api/AB0079/QueryD
    var OrderDrComboGet = '../../../api/AB0079/OrderDrCombo';
    var SectionNoComboGet = '../../../api/AB0079/SectionNoCombo';
    var OrderCodeComboGet = '../../../api/AB0079/OrderCodeCombo';

    var T1GetExcel = '../../../api/AB0079/Excel';

    var T1Name = "每月醫師、科室、藥品醫令統計";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 醫師下拉 combo查詢
    var OrderDrStoreFr = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: OrderDrComboGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    var OrderDrStoreTo = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: OrderDrComboGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    // 科室下拉 combo查詢
    var SectionNoStoreFr = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: SectionNoComboGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    var SectionNoStoreTo = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: SectionNoComboGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    // 院內碼 下拉 combo查詢
    var OrderCodeStoreFr = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: OrderCodeComboGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    var OrderCodeStoreTo = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: OrderCodeComboGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    var sortMethod = Ext.create('Ext.form.RadioGroup', {
        fieldLabel: '統計類別',
        id: 'radioSortMethod',
        vertical: false,
        column: 4,
        items: [
            { name: 'SUM_KIND', boxLabel: '醫師', inputValue: 'D', checked: true },
            { name: 'SUM_KIND', boxLabel: '科室', inputValue: 'S' },
            { name: 'SUM_KIND', boxLabel: '藥品', inputValue: 'M' },
            {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    if ((T1Query.getForm().findField('QryDateFr').getValue() == "" || T1Query.getForm().findField('QryDateFr').getValue() == null) ||
                        (T1Query.getForm().findField('QryDateTo').getValue() == "" || T1Query.getForm().findField('QryDateTo').getValue() == null)) {

                        Ext.Msg.alert('訊息', '需填查詢月份才能查詢');
                    }
                    else {
                        T1Load();
                        msglabel('訊息區:');
                    }
                }
            },
            {
                xtype: 'button',
                text: '清除',
                style: 'margin:0px 5px;',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    msglabel('訊息區:');
                }
            }
        ]
    });

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 85;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,     // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [
                    sortMethod     // 統計類別
                ]
            },
            {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'panel',     // 查詢月份
                        id: 'PanelP2a',
                        border: false,
                        layout: 'hbox',
                        items: [
                            {
                                xtype: 'monthfield',
                                id: 'QryDateFr',
                                name: 'QryDateFr',
                                fieldLabel: '查詢月份',
                                width: 170,
                                labelWidth: 90,
                                fieldCls: 'required',
                                allowBlank: false,
                                padding: '0 4 0 0'
                            },
                            {
                                xtype: 'monthfield',
                                id: 'QryDateTo',
                                name: 'QryDateTo',
                                fieldLabel: '至',
                                width: 90,
                                labelWidth: 12,
                                labelSeparator: '',
                                fieldCls: 'required',
                                allowBlank: false,
                                padding: '0 4 0 0'
                            }, {
                                xtype: 'hidden',
                                id: 'hidQryDateFr',
                                name: 'hidQryDateFr'
                            }, {
                                xtype: 'hidden',
                                id: 'hidQryDateTo',
                                name: 'hidQryDateTo'
                            }
                        ]
                    }
                ]
            }
            ,
            {
                xtype: 'panel',
                id: 'PanelP3',
                border: false,
                layout: 'hbox',
                defaults: {
                    anchor: '100%',
                    editable: true,
                    forceSelection: true,
                    labelAlign: 'right',
                },
                items: [
                    {
                        xtype: 'combobox',
                        store: OrderDrStoreFr,
                        name: 'doctorFr',
                        id: 'doctorFr',
                        width: 280,
                        labelWidth: 90,
                        fieldLabel: '醫師',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        padding: '2 4 0 0'
                    },
                    {
                        xtype: 'combobox',
                        store: OrderDrStoreTo,
                        name: 'doctorTo',
                        id: 'doctorTo',
                        width: 210,
                        labelWidth: 12,
                        fieldLabel: '至',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        labelSeparator: '',
                        queryMode: 'local',
                        padding: '2 4 0 0'
                    }
                ]
            }
            ,
            {
                xtype: 'panel',
                id: 'PanelP4',
                border: false,
                layout: 'hbox',
                defaults: {
                    anchor: '100%',
                    editable: true,
                    forceSelection: true,
                    labelAlign: 'right',
                },
                items: [
                    {
                        xtype: 'combobox',
                        store: SectionNoStoreFr,
                        fieldLabel: '科室',
                        id: 'sectionNoFr',
                        name: 'sectionNoFr',
                        width: 280,
                        labelWidth: 90,
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        padding: '2 4 0 0'
                    },
                    {
                        xtype: 'combobox',
                        store: SectionNoStoreTo,
                        fieldLabel: '至',
                        id: 'sectionNoTo',
                        name: 'sectionNoTo',
                        width: 210,
                        labelWidth: 12,
                        labelSeparator: '',
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        padding: '2 4 0 0'
                    }
                ]
            }
            ,
            {
                xtype: 'panel',
                id: 'PanelP5',
                border: false,
                layout: 'hbox',
                defaults: {
                    anchor: '100%',
                    editable: false,
                    forceSelection: true,
                    labelAlign: 'right',
                },
                items: [
                    {
                        xtype: 'combobox',
                        store: OrderCodeStoreFr,
                        id: 'orderCodeFr',
                        name: 'orderCodeFr',
                        fieldLabel: '藥品院內碼',
                        width: 480,
                        labelWidth: 90,
                        queryMode: 'local',
                        editable: true,
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        padding: '2 4 0 0'
                    },
                    {
                        xtype: 'combobox',
                        store: OrderCodeStoreTo,
                        width: 400,
                        labelWidth: 12,
                        id: 'orderCodeTo',
                        name: 'orderCodeTo',
                        labelSeparator: '',
                        editable: true,
                        fieldLabel: '至',
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        padding: '2 4 0 0'
                    }
                ]
            }
        ]

    });

    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('SUM_KIND').getGroupValue());
        T1Store.getProxy().setExtraParam("p1a", T1Query.getForm().findField('QryDateFr').getRawValue());
        T1Store.getProxy().setExtraParam("p1b", T1Query.getForm().findField('QryDateTo').getRawValue());
        T1Store.getProxy().setExtraParam("p2a", T1Query.getForm().findField('doctorFr').getValue());
        T1Store.getProxy().setExtraParam("p2b", T1Query.getForm().findField('doctorTo').getValue());
        T1Store.getProxy().setExtraParam("p3a", T1Query.getForm().findField('sectionNoFr').getValue());
        T1Store.getProxy().setExtraParam("p3b", T1Query.getForm().findField('sectionNoTo').getValue());
        T1Store.getProxy().setExtraParam("p4a", T1Query.getForm().findField('orderCodeFr').getValue());
        T1Store.getProxy().setExtraParam("p4b", T1Query.getForm().findField('orderCodeTo').getValue());

        T1Store.load({
            params: {
                start: 0
            }
            ,
            callback: function (records, operation, success) {
                if (success) {
                    if (records.length == 0) {
                    } else {
                    }
                } else {
                    Ext.Msg.alert('訊息', 'failure!');
                }
            }
        })

        T1Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export',
                text: '匯出',
                handler: function () {
                    if ((T1Query.getForm().findField('QryDateFr').getValue() == "" || T1Query.getForm().findField('QryDateFr').getValue() == null) ||
                        (T1Query.getForm().findField('QryDateTo').getValue() == "" || T1Query.getForm().findField('QryDateTo').getValue() == null)) {

                        Ext.Msg.alert('訊息', '需填查詢月份才能匯出');
                    }
                    else {
                        var today = getTodayDate();
                        var p = new Array();
                        p.push({ name: 'FN', value: today + '_每月醫師、科室、藥品醫令統計下載.xls' });
                        p.push({ name: 'p0', value: T1Query.getForm().findField('SUM_KIND').getGroupValue() });
                        p.push({ name: 'p1a', value: T1Query.getForm().findField('QryDateFr').getRawValue() });
                        p.push({ name: 'p1b', value: T1Query.getForm().findField('QryDateTo').getRawValue() });
                        p.push({ name: 'p2a', value: T1Query.getForm().findField('doctorFr').getValue() });
                        p.push({ name: 'p2b', value: T1Query.getForm().findField('doctorTo').getValue() });
                        p.push({ name: 'p3a', value: T1Query.getForm().findField('sectionNoFr').getValue() });
                        p.push({ name: 'p3b', value: T1Query.getForm().findField('sectionNoTo').getValue() });
                        p.push({ name: 'p4a', value: T1Query.getForm().findField('orderCodeFr').getValue() });
                        p.push({ name: 'p4b', value: T1Query.getForm().findField('orderCodeTo').getValue() });
                        PostForm(T1GetExcel, p);
                    }
                }
            }
        ]
    });

    var getTodayDate = function () {
        var y = (new Date().getFullYear() - 1911).toString();
        var m = (new Date().getMonth() + 1).toString();
        var d = (new Date().getDate()).toString();
        m = m.length > 1 ? m : "0" + m;
        d = d.length > 1 ? d : "0" + d;
        return y + m + d;
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
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'ORDERCODE',
            width: 90
        }, {
            text: "英文名稱",
            dataIndex: 'ORDERENGNAME',
            width: 240
        }, {
            text: "查詢月份",
            dataIndex: 'CREATEYM',
            width: 68
        }, {
            text: "醫生代碼",
            dataIndex: 'ORDERDR',
            width: 80
        }, {
            text: "醫生姓名",
            dataIndex: 'CHINNAME',
            width: 80
        }, {
            text: "科室",
            dataIndex: 'SECTIONNO',
            width: 50
        }, {
            text: "科室名",
            dataIndex: 'SECTIONNAME',
            width: 120
        }, {
            style: 'text-align:left',
            align: 'right',
            text: "醫囑(住)消耗量",
            dataIndex: 'SUMQTY',
            width: 115
        }, {
            style: 'text-align:left',
            align: 'right',
            text: "醫囑(住)消耗金額",
            dataIndex: 'SUMAMOUNT',
            width: 120
        }, {
            style: 'text-align:left',
            align: 'right',
            text: "醫囑(門)消耗量",
            dataIndex: 'OPDQTY',
            width: 115
        }, {
            style: 'text-align:left',
            align: 'right',
            text: "醫囑(門)消耗金額",
            dataIndex: 'OPDAMOUNT',
            width: 120
        }, {
            text: "統計類別",
            dataIndex: 'DSM',
            width: 80,
            renderer: function (value) {
                if (value == 'D')
                    return "醫生";
                else if (value == 'S')
                    return "科室";
                else if (value == 'M')
                    return "藥品";
            }
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
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
        }]
    });

});
