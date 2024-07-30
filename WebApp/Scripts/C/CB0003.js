Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "品項條碼下載";
    var MatClassGet = '../../../api/CB0003/GetMatclassCombo';
    var T1GetExcel = '../../../api/CB0003/Excel';

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 物料分類清單
    var matClassStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 設定狀態
    var statusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
            { "VALUE": "", "TEXT": "全部" },
            { "VALUE": "Y", "TEXT": "Y 使用中" },
            { "VALUE": "N", "TEXT": "N 停用" }
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
                    store: matClassStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '物料分類',
                    queryMode: 'local',
                    name: 'P0',
                    id: 'P0',
                    padding: '0 4 0 4',
                    lazyRender: true,
                    listener: {
                        afterrender: getMatclassCombo()
                    }
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '院內碼',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 13,
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '中文品名',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    maxLength: 13,
                    padding: '0 4 0 4'
                },
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
                    maxLength: 13,
                    padding: '0 4 0 4'
                },
                {
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

                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        T1Query.getForm().findField('P4').setValue(T1Query.getForm().findField('P4').getValue().toUpperCase());
                        T1Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').setValue('');
                        f.findField('P4').setValue('');
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

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
                    }
                    T1Query.getForm().findField('P0').setValue("");
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var T1Store = Ext.create('WEBAPP.store.CB0003', { // 定義於/Scripts/app/store/CB0003.js 
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P3的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue()
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
        plain: true,
        buttons: [
            {
                itemId: 'export',
                text: '匯出',
                handler: function () {

                    //var length = T1Grid.getStore().data.items.length;
                    //if (length < 1) {
                    //    Ext.Msg.alert('提示', '無資料可供匯出');
                    //    return;
                    //}

                    var today = getTodayDate();
                    var p = new Array();
                    p.push({ name: 'FN', value: today + ' 品項條碼下載.xls' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });
                    PostForm(T1GetExcel, p);
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
            text: "物料分類",
            dataIndex: 'MAT_CLSNAME',
            width: 110
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 120
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 200
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 200
        }, {
            text: "國際條碼",
            dataIndex: 'BARCODE',
            width: 200
        }, {
            text: "條碼類別代碼",
            dataIndex: 'XCATEGORY',
        },
        {
            text: "條碼類別敘述",
            dataIndex: 'DESCRIPT',
        },
        {
            text: "狀態",
            dataIndex: 'STATUS_NAME',
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
            items: [T1Grid]
        }]
    });

    T1Query.getForm().findField('P4').setValue("");
});
