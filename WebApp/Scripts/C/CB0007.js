Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    //var T1Set = '../../../api/CB0006/SetBcItmanagers'; // 新增/修改/刪除
    //var T1Name = "品項管理人員對照維護";
    var UserInidGet = '/api/CB0006/GetUserInid';
    var MatClassGet = '/api/CB0006/GetMatclassCombo';
    var ManagerIdGet = '/api/CB0006/GetManageridCombo';
    var WhnoGet = '/api/CB0006/GetWhnoCombo';

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 物料分類清單
    var matClassStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    // 品項管理員清單
    var managerIdStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    // 品項管理員Grid清單
    var managerIdGridStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    // 庫房清單
    var whnoStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    // 設定狀態
    var statusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
            { "VALUE": "", "TEXT": "全部" },
            { "VALUE": "Y", "TEXT": "已設定" },
            { "VALUE": "N", "TEXT": "未設定" }
        ]
    });


    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
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
            items: [
                {
                    xtype: 'combo',
                    store: whnoStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '庫房別',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    allowBlank: false, // 欄位為必填
                    blankText: "請選擇庫房別",
                    fieldCls: 'required',

                }, {
                    xtype: 'textfield',
                    fieldLabel: '院內碼',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 13,
                    width: 330,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '中文品名',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    maxLength: 100,
                    width: 315,
                    padding: '0 4 0 4'
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '英文品名',
                    name: 'P3',
                    id: 'P3',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'combo',
                    store: statusStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '設定狀態',
                    name: 'P4',
                    id: 'P4',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    width: 150

                }, {
                    xtype: 'combo',
                    store: matClassStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '物料分類',
                    queryMode: 'local',
                    name: 'P5',
                    id: 'P5',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    lazyRender: true,
                    width: 172,
                    labelWidth: 62,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    listener: {
                        afterrender: getMatclassCombo()
                    }
                }, {
                    xtype: 'combo',
                    store: managerIdStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '品項管理員',
                    name: 'P6',
                    id: 'P6',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    width: 315,
                    listener: {
                        //afterrender: getManagerIdCombo()
                    }
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (T1Query.getForm().isValid()) {
                            T1Load();
                        }
                        else {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房別</span>為必填');
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
        }]
    });

    function getUserInid() {
        Ext.Ajax.request({
            url: UserInidGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var value = data.etts[0];
                    T1Query.getForm().findField('P0').setValue(value);
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function getWhnoCombo() {
        Ext.Ajax.request({
            url: WhnoGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var whnos = data.etts;
                    if (whnos.length > 0) {
                        whnoStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < whnos.length; i++) {
                            whnoStore.add({ VALUE: whnos[i].VALUE, TEXT: whnos[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function getMatclassCombo() {
        Ext.Ajax.request({
            url: MatClassGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var matclasses = data.etts;
                    if (matclasses.length > 0) {
                        matClassStore.add({ VALUE: '', TEXT: '全部' });
                        for (var i = 0; i < matclasses.length; i++) {
                            matClassStore.add({ VALUE: matclasses[i].VALUE, TEXT: matclasses[i].TEXT });
                        }

                        T1Query.getForm().findField("P5").setValue("");
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function getManagerIdCombo() {
        Ext.Ajax.request({
            url: ManagerIdGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var managerIds = data.etts;
                    if (managerIds.length > 0) {
                        managerIdStore.add({ VALUE: '', TEXT: '全部' });
                        for (var i = 0; i < managerIds.length; i++) {
                            managerIdStore.add({ VALUE: managerIds[i].VALUE, TEXT: managerIds[i].TEXT });
                        }
                        T1Query.getForm().findField("P6").setValue("");
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function getManagerIdGridCombo() {
        Ext.Ajax.request({
            url: ManagerIdGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var managerIds = data.etts;
                    if (managerIds.length > 0) {
                        managerIdGridStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < managerIds.length; i++) {
                            managerIdGridStore.add({ VALUE: managerIds[i].VALUE, TEXT: managerIds[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    Ext.define('ExcelModel', {
        extend: 'Ext.data.Model',
        Property: 'WH_NO',
        fields: [
            { name: 'WH_NO', cname: '庫房別' },
            { name: 'MAT_CLSNAME', cname: '物料分類' },
            { name: 'MMCODE', cname: '院內碼' },
            { name: 'MANAGERID', cname: '品項管理員' },
            { name: 'MMNAME_C', cname: '中文品名' },
            { name: 'MMNAME_E', cname: '英文品名' }]
    });

    var ExcelStore = Ext.create('Ext.data.Store', {
        model: ExcelModel,
        listeners: {
            beforeload: function (store, options) { },
            load: function (store, records, successful, eOpts) { }
        },
       
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CB0007/GetExcel',
            reader: {
                type: 'json',
                root: 'etts'
            }
        }
    });




    var T1Store = Ext.create('WEBAPP.store.BcItmanager', { // 定義於/Scripts/app/store/BcItmanager.js 
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
        //,buttons: [
        //    {
        //        text: '匯出', border: 1,
        //        style: {
        //            borderColor: '#0080ff',
        //            borderStyle: 'solid'
        //        },
        //        handler: function () {
        //            var e = T1Query.getForm().findField('P0').getValue() + ',' + T1Query.getForm().findField('P1').getValue() + ',' + T1Query.getForm().findField('P2').getValue() + ',' + T1Query.getForm().findField('P3').getValue() + ',' + T1Query.getForm().findField('P4').getValue() + ',' + T1Query.getForm().findField('P5').getValue() + ',' + T1Query.getForm().findField('P6').getValue();
        //            var p = new Array();
        //            p.push({ name: 'FN', value: 'CB0007Excel.xls' }); //檔名
        //            p.push({ name: 'TS', value: e }); //SQL篩選條件
        //            PostForm('/api/CB0007/GetExcel', p);
        //        }
        //    }
        //]
    });

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
            text: "庫房別",
            dataIndex: 'WH_NO',
            width: 80
        }, {
            text: "物料分類",
            dataIndex: 'MAT_CLSNAME',
            width: 100
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 120
        }, {
           text: "管理員代表",
           dataIndex: 'MANAGERNM',
            width: 320
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 150
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150
        }, {
            header: "",
            flex: 1
        }],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
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
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    getWhnoCombo();
    //getManagerIdGridCombo();
    T1Query.getForm().findField('P4').setValue("");
    T1Query.getForm().findField('P0').focus();
});
