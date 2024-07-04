Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.ImageGridField']);

Ext.onReady(function () {

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var data_ym = Ext.getUrlParam('data_ym');

    var mLabelWidth = 70;
    var mWidth = 150;

    var YMComboGet = '../../../api/FA0055/GetYMCombo';
    var YMQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setYMComboData() {
        Ext.Ajax.request({
            url: YMComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var set_ym = data.etts;
                    if (set_ym.length > 0) {
                        for (var i = 0; i < set_ym.length; i++) {
                            YMQueryStore.add({ VALUE: set_ym[i].VALUE, TEXT: set_ym[i].TEXT });
                            if (i == 0) {
                                tmpYM = set_ym[i];
                            }
                        }
                    }
                    T1Query.getForm().findField('P0').setValue(tmpYM);  //預設第一筆最近年月
                    if (data_ym != null && data_ym != undefined && data_ym != '') {
                        T1Query.getForm().findField('P0').setValue(data_ym);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setYMComboData();

    var StatusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { 'VALUE': '', TEXT: '' },
            { 'VALUE': '1', TEXT: '大於等於零' },
            { 'VALUE': '0', TEXT: '小於零' },
        ]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
            { name: 'F2', type: 'string' },     // 期初存量
            { name: 'F3', type: 'string' },     // 期初成本
            { name: 'F4', type: 'string' },     // 自費消耗成本
            { name: 'F5', type: 'string' },     // 自費消耗收入
            { name: 'F6', type: 'string' },     // 健保消耗成本
            { name: 'F7', type: 'string' },     // 健保消耗收入
            { name: 'F8', type: 'string' },     // 全院消耗成本
            { name: 'F9', type: 'string' },     // 全院消耗額收入
            { name: 'F10', type: 'string' },    // 門診耗量
            { name: 'F11', type: 'string' },    // 門診耗量成本
            { name: 'F12', type: 'string' },    // 門診自費耗量
            { name: 'F13', type: 'string' },    // 門診健保耗量
            { name: 'F14', type: 'string' },    // 住院耗量
            { name: 'F15', type: 'string' },    // 住院耗量成本
            { name: 'F16', type: 'string' },    // 住院自費耗量
            { name: 'F17', type: 'string' },    // 住院健保耗量
            { name: 'F18', type: 'string' },    // 期末存量
            { name: 'F19', type: 'string' },    // 期末存量成本(表)
            { name: 'F20', type: 'string' },    // 健保價
            { name: 'F21', type: 'string' },    // 自費價
            { name: 'F22', type: 'string' },    // 進價
            { name: 'F23', type: 'string' },    // 移動加權平均單價
            { name: 'F24', type: 'string' },    // 上月移動加權平均單價
            { name: 'F25', type: 'string' },    // 英文名稱
            { name: 'F26', type: 'string' },    // 健保碼
            { name: 'F27', type: 'string' },    // 扣庫單位
            { name: 'F28', type: 'string' },    // 全院停用碼
            { name: 'DATA_YM', type: 'string' },    // 查詢條件月結年月
            { name: 'F29', type: 'string' },    // 其他耗量
            { name: 'F30', type: 'string' },    // 其他消耗成本
            { name: 'F31', type: 'string' },    // 急診耗量
            { name: 'F32', type: 'string' },    // 急診耗量成本
            { name: 'F33', type: 'string' },    // 急診自費耗量
            { name: 'F34', type: 'string' },    // 急診健保耗量
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0055/GetDetails',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    data_ym: T1Query.getForm().findField('P0').getValue(),
                    status: T1Query.getForm().findField('P1').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '匯出',
                id: 'excel',
                name: 'excel',
                handler: function () {

                    if (T1Query.getForm().findField('P0').getValue() == '' ||
                        T1Query.getForm().findField('P0').getValue() == null) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>月結年月</span>為必填');
                        return;
                    }

                    var fn = T1Query.getForm().findField('P0').getValue() + '_藥品消耗月結報表.xls'
                    var p = new Array();
                    p.push({ name: 'FN', value: fn }); //檔名
                    p.push({ name: 'data_ym', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件
                    p.push({ name: 'status', value: T1Query.getForm().findField('P1').getValue() }); //SQL篩選條件
                    PostForm('/api/FA0055/GetDetailsExcel', p);
                }
            }
        ]
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    store: YMQueryStore,
                    name: 'P0',
                    id: 'P0',
                    fieldLabel: '月結年月',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: false,
                    fieldCls: 'required',
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    width:150,
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                },
                {
                    xtype: 'combo',
                    store: StatusStore,
                    name: 'P1',
                    id: 'P1',
                    fieldLabel: '期末存量',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    width: 180,
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {

                        if (T1Query.getForm().findField('P0').getValue() == '' || 
                            T1Query.getForm().findField('P0').getValue() == null) {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>月結年月</span>為必填');
                            return;
                        }

                        T1Load(true);
                        msglabel('');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('');
                    }
                }
            ]
        }]
    });
   
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
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
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            },
            {
                text: "期初存量",
                dataIndex: 'F2',
                width: 120,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "期初成本",
                dataIndex: 'F3',
                width: 120,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "自費消耗成本",
                dataIndex: 'F4',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "自費消耗收入",
                dataIndex: 'F5',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "健保消耗成本",
                dataIndex: 'F6',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "健保消耗收入",
                dataIndex: 'F7',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "全院消耗成本",
                dataIndex: 'F8',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "全院消耗額收入",
                dataIndex: 'F9',
                width: 110,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "門診耗量",
                dataIndex: 'F10',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "門診耗量成本",
                dataIndex: 'F11',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "門診自費耗量",
                dataIndex: 'F12',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "急診健保耗量",
                dataIndex: 'F31',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "急診耗量成本",
                dataIndex: 'F32',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "急診自費耗量",
                dataIndex: 'F33',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "急診健保耗量",
                dataIndex: 'F34',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "住院耗量",
                dataIndex: 'F14',
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "住院耗量成本",
                dataIndex: 'F15',
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "住院自費耗量",
                dataIndex: 'F16',
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "住院健保耗量",
                dataIndex: 'F17',
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "期末存量",
                dataIndex: 'F18',
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "期末存量成本(表)",
                dataIndex: 'F19',
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "健保價",
                dataIndex: 'F20',
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "自費價",
                dataIndex: 'F21',
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "進價",
                dataIndex: 'F22',
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "移動加權平均單價",
                dataIndex: 'F23',
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "上月移動加權平均單價",
                dataIndex: 'F24',
                width: 140,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "英文名稱",
                dataIndex: 'F25',
                width: 100
            },
            {
                text: "健保碼",
                dataIndex: 'F26',
                width: 100
            },
            {
                text: "扣庫單位",
                dataIndex: 'F27',
                width: 100
            },
            {
                text: "全院停用碼",
                dataIndex: 'F28',
                width: 100
            },
            {
                text: "其他耗量",
                dataIndex: 'F29',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "其他消耗成本",
                dataIndex: 'F30',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "查詢條件月結年月",
                dataIndex: 'DATA_YM',
                width: 120,
                hidden: true
            },
            
        ]
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
            itemId: 'form',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });


   
});