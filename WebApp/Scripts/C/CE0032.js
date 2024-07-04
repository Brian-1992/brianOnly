Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Get = '/api/CE0032/GetQueryData';
    var reportUrl = '/Report/C/CE0032.aspx';
    var T1RecLength = 0;
    var T1LastRec = null;
    var T1GetExcel = '/api/CE0032/Excel';

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            'MMCODE',
            'MMNAME',
            'STORE_QTY',
            'CHK_UID',
            'CHK_UID_NAME',
            'STORE_LOC_QTY',
            'CHK_QTY',
            'GAP_T'
        ]
    });

    var st_manager = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0032/GetManagerCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });
    function setMatClassCombo() {
        Ext.Ajax.request({
            url: '/api/CE0032/GetMatClassCombo',
            method: reqVal_p,
            success: function (response) {
                st_matclass.removeAll();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    st_matclass.add({ VALUE: '', COMBITEM: '' });
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            st_matclass.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setMatClassCombo();

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    chkYM: vChkYM,
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            timeout: 1800000,
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'PUT',
                delete: 'DELETE'
            },
            url: '/api/CE0032/GetQueryData',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    function showReport() {
        if (!win) {
            var p1 = T1Query.getForm().findField('P1').getValue() == null ? '' : T1Query.getForm().findField('P1').getValue();
            var p2 = T1Query.getForm().findField('P2').getValue() == null ? '' : T1Query.getForm().findField('P2').getValue();
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                    + '?chkYM=' + vChkYM
                    + '&p1=' + p1
                    + '&p2=' + p2
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

    var mLabelWidth = 60;
    var mWidth = 180;
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
                    fieldLabel: '盤點年月',
                    name: 'CHK_YM',
                    id: 'CHK_YM',
                    width: 130,
                    padding: '0 10 0 0',
                    fieldCls: 'required',
                    value: new Date()
                }, {
                    xtype: 'combo',
                    fieldLabel: '盤點人員',
                    name: 'P1',
                    id: 'P1',
                    width: 200, labelWidth: 70,
                    store: st_manager,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    valueField: 'VALUE'
                }, {
                    xtype: 'combo',
                    fieldLabel: '物料分類代碼',
                    name: 'P2',
                    id: 'P2',
                    width: 250, labelWidth: 110,
                    store: st_matclass,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    margin: '0 5 0 20',
                    handler: function () {                        
                        vChkYM = T1Query.getForm().findField('CHK_YM').getValue();

                        if (vChkYM === null) {
                            Ext.Msg.alert('訊息', "請點選盤點年月");
                        }
                        else {
                            vChkYM = (vChkYM.getFullYear() - 1911).toString() + ((vChkYM.getMonth() + 1) > 9 ? '' : '0') + (vChkYM.getMonth() + 1);
                            T1Load();
                        }

                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();

                        vChkYM = "";
                        f.reset();

                        f.findField('CHK_YM').focus(); // 進入畫面時輸入游標預設在D0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出',
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '中央庫房盤點資料彙總表.xls' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('CHK_YM').rawValue });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');

                }
            },{
                text: '列印', handler: function () {
                    if (T1Store.getCount() > 0) {
                        showReport();
                    }
                    else {
                        Ext.Msg.alert('訊息', '無資料可列');
                    }
                }
            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
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
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "品名",
                dataIndex: 'MMNAME',
                style: 'text-align:left',
                align: 'left',
                width: 350
            }, {
                text: "電腦量",
                dataIndex: 'STORE_QTY',
                style: 'text-align:right',
                align: 'right',
                width: 80
            }, {
                text: "盤點人員",
                dataIndex: 'CHK_UID_NAME',
                style: 'text-align:left',
                align: 'left',
                width: 120
            }, {
                text: "儲位 - 量",
                dataIndex: 'STORE_LOC_QTY',
                style: 'text-align:left',
                align: 'left',
                width: 180
            }, {
                text: "實盤量",
                dataIndex: 'CHK_QTY',
                style: 'text-align:right',
                align: 'right',
                width: 80
            }, {
                text: "差異量",
                dataIndex: 'GAP_T',
                style: 'text-align:right',
                align: 'right',
                width: 80
            }, {
                header: "",
                flex: 1
            }],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
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
        }
        ]
    });

    T1Query.getForm().findField('CHK_YM').focus();
});