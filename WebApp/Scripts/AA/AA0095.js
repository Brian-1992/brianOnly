Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var reportUrl = '/Report/A/AA0095.aspx';
    var T1Name = "近效期列表";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //庫別代碼Store
    var st_WHNO = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0095/GetWHNO',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true,
    });

    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
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
                    fieldLabel: '庫別代碼',
                    store: st_WHNO,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    anyMatch: true,
                    queryMode: 'local',
                    name: 'P0',
                    id: 'P0',
                    width: 200,
                    allowBlank : false,
                    fieldCls: 'required',
                    listConfig:
                    {
                        width: 200
                    },
                    matchFieldWidth: false
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '月份別',
                    name: 'P1',
                    id: 'P1',
                    allowBlank : false,
                    fieldCls: 'required',
                    width: 160
                }, {
                    xtype: 'textfield',
                    id: 'P2',
                    name: 'P2',
                    fieldLabel: '藥品代碼',
                    width: 170,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    id: 'P3',
                    name: 'P3',
                    fieldLabel: '至',
                    width: 130,
                    labelWidth: 10,
                    labelSeparator: '',
                    padding: '0 4 0 4'
                }, 
                {
                    xtype: 'button',
                    text: '查詢',
                    formBind: true,
                    handler: function () {
                        showReport();
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                    }
                }
            ]
        }]
    });

    Ext.define('AA0095Report_Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'EXP_DATE1', type: 'string' },
            { name: 'LOT_NO1', type: 'string' },
            { name: 'EXP_DATE2', type: 'string' },
            { name: 'LOT_NO2', type: 'string' },
            { name: 'EXP_DATE3', type: 'string' },
            { name: 'LOT_NO3', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'EXP_QTY', type: 'int' },
            { name: 'MEMO', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'AA0095Report_Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0095/SearchReportData',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getRawValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue()
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
                itemId: 't1print', text: '列印', disabled: true, handler: function () {
                    //if (T11Store.getCount() > 0)
                    showReport();
                    //else
                    //    Ext.Msg.alert('訊息', '請先建立明細資料');
                }
            }
        ]
    });

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
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        if (T1LastRec) {
            T1Grid.down('#t1print').setDisabled(false);
        }
        else {
            T1Form.getForm().reset();
            T1Grid.down('#t1print').setDisabled(true);
        }
    }

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        setFormT1a();
    }

    function showReport() {
        if (!win) {
            var np = {
                p0: T1Query.getForm().findField('P0').getValue(),
                p1: T1Query.getForm().findField('P1').getRawValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p3: T1Query.getForm().findField('P3').getValue(),
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
                itemId: 'tGrid',
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
                            itemId: 't1Grid',
                            region: 'center',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            border: false,
                            items: [T1Grid]
                        }]
                }]
            }
        ]
    });
    //申請單狀態預設為1，空白表示全選
    //T1Query.getForm().findField('P4').setValue(1);
    //T1Query.getForm().findField('P0').focus();
});
