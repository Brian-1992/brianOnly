Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var reportUrl = '/Report/A/AA0013.aspx';
    var T1Name = "列印申請單";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    
    var isGas = Ext.getUrlParam('isgas');

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    function sanitize(value) {
        
        if (Array.isArray(value)) {
            var tempResult = [];
            for (var i = 0; i < value.length; i++) {
                var temp = value[i];

                temp = temp.replaceAll('|', '');
                temp = temp.replaceAll('&', '');
                temp = temp.replaceAll(';', '');
                temp = temp.replaceAll('$', '');
                temp = temp.replaceAll('%', '');
                temp = temp.replaceAll('@', '');
                temp = temp.replaceAll("'", '');
                temp = temp.replaceAll('"', '');
                temp = temp.replaceAll('\'', '');
                temp = temp.replaceAll('\"', '');
                temp = temp.replaceAll('<', '');
                temp = temp.replaceAll('>', '');
                temp = temp.replaceAll('(', '');
                temp = temp.replaceAll(')', '');
                temp = temp.replaceAll('+', '');
                temp = temp.replaceAll('\r', '');
                temp = temp.replaceAll('\n', '');
                temp = temp.replaceAll(',', '');
                temp = temp.replaceAll('\\', '');

                tempResult.push(temp);
            }
            return tempResult;
        }

        value = value.replaceAll('|', '');
        value = value.replaceAll('&', '');
        value = value.replaceAll(';', '');
        value = value.replaceAll('$', '');
        value = value.replaceAll('%', '');
        value = value.replaceAll('@', '');
        value = value.replaceAll("'", '');
        value = value.replaceAll('"', '');
        value = value.replaceAll('\'', '');
        value = value.replaceAll('\"', '');
        value = value.replaceAll('<', '');
        value = value.replaceAll('>', '');
        value = value.replaceAll('(', '');
        value = value.replaceAll(')', '');
        value = value.replaceAll('+', '');
        value = value.replaceAll('\r', '');
        value = value.replaceAll('\n', '');
        value = value.replaceAll(',', '');
        value = value.replaceAll('\\', '');

        return value;

    }

    //申請單號Store
    var st_docno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            params: {isGas: isGas},
            url: '/api/AA0013/GetDocnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true,
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件值代入參數
                var np = {
                    //申請單位
                    p0: T1Query.getForm().findField('P0').getValue(),
                    //物料分類
                    p1: T1Query.getForm().findField('P1').getValue(),
                    //申請單狀態
                    p4: sanitize(T1Query.getForm().findField('P4').getValue()),
                };
                Ext.apply(store.proxy.extraParams, np);

                st_docno.getProxy().setExtraParam('isGas', isGas);
            }
        }
    });

    //申請單類別Store
    var st_docType = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            param: { isGas: isGas },
            url: '/api/AA0013/GetDocType',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true,
        listeners: {
            beforeload: function () {
                st_docType.getProxy().setExtraParam('isGas', isGas);
            },
        }
    });

    //申請單狀態Store
    var st_Flowid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            param: { isGas: isGas },
            url: '/api/AA0013/GetFlowidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true,
        
        listeners: {
            beforeload: function () {
                st_Flowid.getProxy().setExtraParam('isGas', isGas);
            },
            load: function (store, records, successful, eOpts) {
                store.insert(0, { TEXT: '', VALUE: '' });
            }
        }
    });

    //物料分類Store
    var st_matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0013/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            beforeload: function () {
                st_matclass.getProxy().setExtraParam('isGas', isGas);
            },
            load: function (store, options) {
                store.insert(0, { TEXT: '', VALUE: '' });
                var MatClassCount = store.getCount();
                var combo_P1 = T1Query.getForm().findField('P1');
                if (MatClassCount == 1) {
                    //1筆資料時將                    
                    combo_P1.setDisabled(true);
                    combo_P1.setValue('02');
                }
                else {
                    combo_P1.setDisabled(false);
                }
            }
        },
        autoLoad: true
    });

    //申請單位Store
    var st_AppDept = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0013/GetAppDeptCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                store.insert(0, { TEXT: '', VALUE: '' });
                var AppDeptCount = store.getCount();
                if (!AppDeptCount) {
                    T1Query.getForm().findField('P0').setDisabled(true);
                }
                else {
                    T1Query.getForm().findField('P0').setDisabled(false);
                }
            }
        },
        autoLoad: true
    });

    var mLabelWidth = 80;
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
            bodyStyle: 'padding: 3px 0px;',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '入庫庫房',
                    store: st_AppDept,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    anyMatch: true,
                    //multiSelect: true,
                    queryMode: 'local',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    name: 'P0',
                    id: 'P0',
                    width: 250,
                    listConfig:
                    {
                        width: 250
                    },
                    matchFieldWidth: false,
                    disabled: true,
                    listeners: {
                        select: function (ele, newValue, oldValue) {
                            //st_docno.load();
                        }
                    }
                }, {
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    store: st_matclass,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    anyMatch: true,
                    //multiSelect: true,
                    queryMode: 'local',
                    name: 'P1',
                    id: 'P1',
                    width: 250,
                    //allowBlank: false,
                    //fieldCls: 'required',
                    listeners: {
                        select: function (ele, newValue, oldValue) {
                            //st_docno.load();
                        }
                    }
                }, {
                    xtype: 'datefield',
                    id: 'P2',
                    name: 'P2',
                    fieldLabel: '申請日期',
                    width: 180,
                    allowBlank: false,
                    vtype: 'dateRange',
                    dateRange: { end: 'P3' },
                    padding: '0 4 0 4',
                    fieldCls: 'required'
                }, {
                    xtype: 'datefield',
                    id: 'P3',
                    name: 'P3',
                    fieldLabel: '至',
                    width: 110,
                    labelWidth: 10,
                    labelSeparator: '',
                    //allowBlank: false,
                    vtype: 'dateRange',
                    dateRange: { begin: 'P2' },
                    padding: '0 4 0 4'
                }
            ]
        },
        {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 0px;',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '申請單狀態',
                    store: st_Flowid,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    multiSelect: true,
                    name: 'P4',
                    id: 'P4',
                    width: 250,
                    labelWidth: 80,
                    listeners: {
                        select: function (ele, newValue, oldValue) {
                            //st_docno.load();
                        }
                    }
                }, {
                    xtype: 'combo',
                    fieldLabel: '申請單類別',
                    store: st_docType,
                    name: 'P5',
                    id: 'P5',
                    width: 250,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    //multiSelect: true,
                    allowBlank: false,
                    listConfig:
                    {
                        width: 200
                    },
                    matchFieldWidth: false,
                    fieldCls: 'required'
                }, {
                    xtype: 'datefield',
                    id: 'P6',
                    name: 'P6',
                    fieldLabel: '核撥日期',
                    width: 180,
                    vtype: 'dateRange',
                    dateRange: { end: 'P7' },
                    padding: '0 4 0 4'
                }, {
                    xtype: 'datefield',
                    id: 'P7',
                    name: 'P7',
                    fieldLabel: '至',
                    width: 110,
                    labelWidth: 10,
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'P6' },
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    style: 'margin:0px 5px 0px 30px;',
                    handler: function () {
                        if (T1Query.getForm().findField('P2').validate()
                            && T1Query.getForm().findField('P3').validate() && T1Query.getForm().findField('P5').validate()) {
                            T1Store.load();
                        }
                        else {
                            Ext.Msg.alert('提醒', '[申請單類別]、[申請日期]為必填條件');
                        }

                    }
                },
                {
                    xtype: 'button',
                    text: '預覽',
                    style: 'margin:0px 5px;',
                    handler: function () {
                        var selection = T1Grid.getSelection();
                        if (selection.length) {
                            let DOCNO = '';

                            //selection.map(item => {
                            //    DOCNO += item.get('DOCNO') + ',';
                            //});
                            $.map(selection, function (item, key) {
                                DOCNO += item.get('DOCNO') + ',';
                            })
                            showReport(DOCNO);
                        }
                        else {
                            Ext.Msg.alert('提醒', '尚未勾選任何申請單');
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '列印',
                    style: 'margin:0px 5px;',
                    handler: function () {
                        var selection = T1Grid.getSelection();
                        if (selection.length) {
                            let DOCNO = '';

                            //selection.map(item => {
                            //    DOCNO += item.get('DOCNO') + ',';
                            //});
                            $.map(selection, function (item, key) {
                                DOCNO += item.get('DOCNO') + ',';
                            })
                            PrintReport(DOCNO);
                        }
                        else {
                            Ext.Msg.alert('提醒', '尚未勾選任何申請單');
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    style: 'margin:0px 5px;',
                    handler: function () {
                        st_docno.load();
                        var f = this.up('form').getForm();
                        //判斷物料分類控制項，如果控制項的disabled為True(禁止使用者選擇)，清除後將原本的Value重新帶入
                        var f_P1 = T1Query.getForm().findField('P1');
                        var f_IsP1Disalbed = f_P1.disabled;
                        var f_P1Value = f_P1.getValue();
                        f.reset();
                        if (f_IsP1Disalbed) {
                            f_P1.setValue(f_P1Value);
                        }
                        f.findField('P0').focus();
                    }
                }
            ]
        }]
    });

    Ext.define('AA0013Report_Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'APPID', type: 'string' },
            { name: 'APPDEPT', type: 'string' },
            { name: 'APPLY_KIND', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'LIST_ID', type: 'string' },
            { name: 'DIS_USER', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'APPQTY', type: 'int' },
            { name: 'APVQTY', type: 'int' },
            { name: 'ACKQTY', type: 'string' },
            { name: 'EXPT_DISTQTY', type: 'string' },
            { name: 'BW_MQTY', type: 'string' },
            { name: 'APLYITEM_NOTE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_STOREID', type: 'string' },
            { name: 'FLOWID_N', type: 'string' },
            { name: 'APPDEPT_N', type: 'string' },
            { name: 'APPLY_KIND_N', type: 'string' },
            { name: 'MATCLASS_N', type: 'string' },
            { name: 'LIST_ID_N', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'AA0013Report_Model',
        pageSize: 70, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            //url: '/api/AA0013/SearchPrintData',
            url: '/api/AA0013/AllM',
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
                    p1: T1Query.getForm().findField('P1').getValue(),
                    //p2: T1Query.getForm().findField('P2').getValue(),
                    p2: T1Query.getForm().findField('P2').rawValue,
                    //p3: T1Query.getForm().findField('P3').getValue(),
                    p3: T1Query.getForm().findField('P3').rawValue,
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);

                T1Store.getProxy().setExtraParam('isGas', isGas);
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
        //buttons: [
        //    {
        //        itemId: 't1print', text: '列印', disabled: true, handler: function () {
        //            //if (T11Store.getCount() > 0)
        //            //showReport();
        //            //else
        //            //    Ext.Msg.alert('訊息', '請先建立明細資料');
        //        }
        //    }
        //]
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
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        selModel: {
            checkOnly: false,
            allowDeselect: true,
            injectCheckbox: 'first',
            mode: 'MULTI',
            listeners: {

            }
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "申請單號",
            dataIndex: 'DOCNO',
            width: 160
        }, {
            text: "類別",
            dataIndex: 'APPLY_KIND_N',
            width: 80
        }, {
            text: "狀態",
            dataIndex: 'FLOWID_N',
            width: 80
        }, {
            text: "申請時間",
            dataIndex: 'APPTIME',
            width: 100
        }, {
            text: "出庫庫房",
            dataIndex: 'FRWH_NAME',
            width: 140
        }, {
            text: "入庫庫房",
            dataIndex: 'TOWH_NAME',
            width: 140
        }, {
            text: "物料分類",
            dataIndex: 'MATCLASS_N',
            width: 100
        }, {
            text: "是否列印",
            dataIndex: 'LIST_ID_N',
            width: 100 
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        //if (T1LastRec) {
        //    T1Grid.down('#t1print').setDisabled(false);
        //}
        //else {
        //    T1Form.getForm().reset();
        //    T1Grid.down('#t1print').setDisabled(true);
        //}
    }

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        setFormT1a();
    }

    function showReport(DOCNO) {
        
        if (!win) {
            var np = {
                p0: T1Query.getForm().findField('P0').getValue(),
                p1: T1Query.getForm().findField('P1').getValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p3: T1Query.getForm().findField('P3').getValue(),
                p4: T1Query.getForm().findField('P4').getValue(),
                p5: DOCNO,
                p6: T1Query.getForm().findField('P6').getValue(),
                p7: T1Query.getForm().findField('P7').getValue(),
                Action: 0
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1
                + '&p2=' + np.p2 + '&p3=' + np.p3 + '&p4=' + np.p4
                + '&p5=' + np.p5 + '&p6=' + np.p6 + '&p7=' + np.p7
                + '&Action=' + np.Action
                + '&isGas=' + isGas
                + '&Order=storeloc" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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

    function PrintReport(DOCNO) {
        
        if (!win) {
            var np = {
                p0: T1Query.getForm().findField('P0').getValue(),
                p1: T1Query.getForm().findField('P1').getValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p3: T1Query.getForm().findField('P3').getValue(),
                p4: T1Query.getForm().findField('P4').getValue(),
                p5: DOCNO,
                p6: T1Query.getForm().findField('P6').getValue(),
                p7: T1Query.getForm().findField('P7').getValue(),
                Action: 1,
                isGas: isGas
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '&p4=' + np.p4 + '&p5=' + np.p5 + '&p6=' + np.p6 + '&p7=' + np.p7 + '&Action=' + np.Action +'&isGas='+np.isGas+ '&Order=storeloc" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
        this.up('window').destroy();
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
    T1Query.getForm().findField('P4').setValue(1);
    T1Query.getForm().findField('P0').focus();
});
