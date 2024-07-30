Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var reportUrl = '/Report/A/AA0107.aspx';
    // var T1Get = '/api/AA0107/All'; // 查詢(改為於store定義)
    var T1Name = "期末比值異常單位查詢";
    
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_Chkym = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0107/GetChkymCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    var mLabelWidth = 70;
    var mWidth = 150;

    // 查詢欄位
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
                    xtype: 'monthfield',
                    fieldLabel: '年月',
                    name: 'P0',
                    id: 'P0',
                    fieldCls: 'required',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    padding: '0 4 0 4',
                    value: new Date()
                }, {
                    xtype: 'radiogroup',
                    fieldLabel: '單位類別',
                    name: 'QtyType',
                    width: 300,
                    items: [
                        { boxLabel: '異常比值<0', width: 100, name: 'QtyType', inputValue: '0', checked: true },
                        { boxLabel: '異常比值>1', width: 100, name: 'QtyType', inputValue: '1' }
                    ]
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (
                            (this.up('form').getForm().findField('P0').getValue() == '' || this.up('form').getForm().findField('P0').getValue() == null)
                        ) {
                            Ext.Msg.alert('提醒', '年月不可空白');
                        }
                        else {
                            T1Load();
                            msglabel('訊息區:');
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                },
                {
                    xtype: 'button',
                    text: '列印',
                    id: 'T1btn1',
                    disabled: true,
                    handler: function () {
                        showReport();
                    }
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '(只能查已月結之資料)',
                    labelWidth:150,
                    labelSeparator: '',
                    labelStyle: 'font-weight: bold; color: blue;',
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '異常比值： 盤點量/耗用量 (註：耗用量不為0)',
                    labelWidth: 300,
                    labelSeparator: '',
                    labelStyle: 'font-weight: bold; color: blue;',
                }
            ]
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'INID', type: 'string' },
            { name: 'INID_NAME', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'INID', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0107/AllM',
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
                    p1: T1Query.getForm().findField('QtyType').getValue()['QtyType']
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            },
            callback: function (records) {
                if (records.length > 0) {
                    Ext.getCmp('T1btn1').setDisabled(false);
                }
            }
        });
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
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
            text: "單位代碼",
            dataIndex: 'INID',
            width: 120
        }, {
            text: "單位名稱",
            dataIndex: 'INID_NAME',
            width: 300
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {

                }
            }
        }
    });
    function showReport() {
        if (!win) {
            var p0 = T1Query.getForm().findField('P0').rawValue == null ? '' : T1Query.getForm().findField('P0').rawValue;
            var p1 = T1Query.getForm().findField('QtyType').getValue()['QtyType'];

            var qstring = '?p0=' + p0 + '&p1=' + p1 ;

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + qstring + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
    //view 
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
            items: [{
                //  xtype:'container',
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
                    }/*,
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }*/
                ]
            }]
        }
        ]
    });
    
    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
});
