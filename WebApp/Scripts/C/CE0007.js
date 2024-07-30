Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app',
        'TAB': '/Scripts'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var tabFlag = true;
    var viewModel = Ext.create('WEBAPP.store.CE0007VM');
    var CE0004viewModel = Ext.create('WEBAPP.store.CE0004VM');

    var T1Store = viewModel.getStore('CHK_MAST');
    function T1Load() {
        T1Store.getProxy().setExtraParam("WH_NO", T1Query.getForm().findField('WH_NO').getValue());
        T1Store.getProxy().setExtraParam("CHK_YM", T1Query.getForm().findField('CHK_YM').rawValue);
        T1Tool.moveFirst();
    }

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: '/api/CE0002/GetWhnoCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    //                    whnoQueryStore.add({ WH_NO: '', WH_KIND: '', WH_NAME: '', WH_GRADE: '' });
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add(wh_nos[i]);
                        }

                        T1Query.getForm().findField('WH_NO').setValue(wh_nos[0].WH_NO);
                        T1Load(true);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    function setWhno(args) {
        if (args.WH_NO !== '') {
            T1Query.getForm().findField("WH_NO").setValue(args.WH_NO);
        }
    }

    function getDefaultValue() {
        var yyyy = 0;
        var m = 0;
        yyyy = new Date().getFullYear() - 1911;
        m = new Date().getMonth() + 1;
        var mm = m >= 10 ? m.toString() : "0" + m.toString();
        return yyyy.toString() + mm;
    }

    // 查詢欄位
    var mLabelWidth = 90;
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
        items: [
            {

                xtype: 'panel',
                border: false,
                layout: 'hbox',
                width:'100%',
                padding: '0 0 5 0',
                items: [
                    {
                        xtype: 'combo',
                        store: whnoQueryStore,
                        name: 'WH_NO',
                        id: 'WH_NO',
                        fieldLabel: '庫房代碼',
                        displayField: 'WH_NAME',
                        valueField: 'WH_NO',
                        queryMode: 'local',
                        anyMatch: true,
                        allowBlank: false,
                        width:250,
                        fieldCls: 'required',
                        typeAhead: true,
                        forceSelection: true,
                        triggerAction: 'all',
                        multiSelect: false,
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
                    },
                    //wh_NoCombo,
                    //{
                    //    xtype: 'button',
                    //    itemId: 'btnWh_no',
                    //    iconCls: 'TRASearch',
                    //    handler: function () {
                    //        var f = T1Query.getForm();
                    //        popWh_noForm(viewport, '/api/CE0014/GetWhno', { WH_NO: f.findField("WH_NO").getValue() }, setWhno);
                    //    }
                    //},
                    {
                        xtype: 'monthfield',
                        fieldLabel: '盤點年月',
                        name: 'CHK_YM',
                        //value: '10806',
                        //enforceMaxLength: true,
                        maxLength: 5,
                        width: 170,
                        padding: '0 4 0 4',
                        value: getDefaultValue()
                    },
                    {
                        xtype: 'component',
                        autoEl: {
                            tag: 'span',
                            style: 'margin-top: 3px;color:#194c80',
                            html: '(民國年月yyymm)'
                        }
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤點單負責人',
                        value: session['UserName'],
                        width: 170,
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            //T2Store.removeAll();
                            T1Grid.getSelectionModel().deselectAll();
                            if (!T1Query.getForm().findField('WH_NO').getValue()) {
                                Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
                                return;
                            }
                            T1Load();
                            msglabel('訊息區:');
                            //Ext.getCmp('btnUpdate').setDisabled(true);
                            //Ext.getCmp('btnDel').setDisabled(true);
                            //Ext.getCmp('btnSubmit').setDisabled(true);
                            //Ext.getCmp('btnAdd2').setDisabled(true);
                            //Ext.getCmp('btnUpdate2').setDisabled(true);
                            //Ext.getCmp('btnDel2').setDisabled(true);
                            //Ext.getCmp('btnCopy').setDisabled(true);
                            //Ext.getCmp('eastform').collapse();
                        }
                    },
                    {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            //f.findField('WH_no').focus(); // 進入畫面時輸入游標預設
                            msglabel('訊息區:');
                        }
                    },
                    {
                        xtype: 'component',
                        flex: 1
                    },
                    {
                        xtype: 'container',
                        layout: 'hbox',
                        right: '100%',
                        items: [
                            {
                                xtype: 'button',
                                text: '三盤管理',
                                handler: function () {
                                    parent.link2('/Form/Index/CE0008', ' 三盤管理(CE0008)', true);
                                    //parent.link2('/Form/Index/CE0016','初盤數量輸入作業');
                                }
                            }
                        ]
                    }
                ]
            }
        ]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            //    {
            //        dock: 'top',
            //        xtype: 'toolbar',
            //        layout: 'fit',
            //        items: [T1Query]
            //    },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        //selModel: {
        //    checkOnly: false,
        //    injectCheckbox: 'first',
        //    mode: 'MULTI'
        //},
        //selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "庫房代碼",
                dataIndex: 'CHK_WH_NO',
                width: 100
            },
            {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                width: 100
            },
            {
                text: "盤點年月日",
                dataIndex: 'CHK_YM',
                width: 80
            },
            {
                text: "庫別級別",
                dataIndex: 'WH_GRADE_NAME',
                width: 100
            },
            {
                text: "庫別分類",
                dataIndex: 'WH_KIND_NAME',
                width: 80,
            },
            {
                text: "盤點期",
                dataIndex: 'CHK_PERIOD_NAME',
                width: 80,
            },
            {
                text: "盤點類別",
                dataIndex: 'CHK_TYPE_NAME',
                width: 100,
                //align: 'right'
            },
            {
                text: "物料分類",
                dataIndex: 'CHK_CLASS_NAME',
                width: 100,
            },
            {
                text: "盤點量/總量",
                dataIndex: 'QTY',
                width: 100,
            },
            {
                text: "盤點單號",
                dataIndex: 'CHK_NO',
                width: 150,
                renderer: function (val, meta, record) {
                    if (val != null)
                        //return '<a href="javascript:popWinForm(' + val + ",'" + record.data['TMPKEY'] + "')\" >" + val + '</a>';
                        //return '<a href="javascript:popWinForm(' + val + ')" >' + val + '</a>';
                        return "<a href=\"javascript:popWinForm('" + val + "')\" >" + val + '</a>';
                }
            },
            //{
            //    text: "負責人員",
            //    dataIndex: 'CHK_KEEPER',
            //    width: 100,
            //},
            {
                text: "狀態",
                dataIndex: 'CHK_STATUS_NAME',
                width: 100,
            },
            {
                text: "",
                dataIndex: 'CHK_LEVEL',
                width: 80,
                renderer: function (val, meta, record) {
                    return '<a href=javascript:void(0)>盤點情形</a>';
                }
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                var columns = T1Grid.getColumns();
                var index = getColumnIndex(columns, 'CHK_LEVEL');
                msglabel('');
                if (index != cellIndex) {
                    return;
                }
                T1LastRec = record;
                T3Load();
                statusWindow.show();
            },
        }
    });
    function getColumnIndex(columns, dataIndex) {
        var index = -1;
        for (var i = 0; i < columns.length; i++) {
            if (columns[i].dataIndex == dataIndex) {
                index = i;
            }
        }

        return index;
    }
    //#region 盤點情形
    var T3Store = CE0004viewModel.getStore('DONE_STATUS');
    function T3Load() {

        T3Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);

        T3Tool.moveFirst();
    }
    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        border: false,
        plain: true,
        displayInfo: true
    });
    var T3Grid = Ext.create('Ext.grid.Panel', {
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: 325,
        layout: 'fit',
        columns: [
            {
                text: "姓名",
                dataIndex: 'CHK_UID_NAME',
                width: 170,
                //text: "員工代碼",
                //dataIndex: 'CHK_UID',
                //width: 170,
                //renderer: function (val, meta, record) {
                //    return record.data.CHK_UID + ' ' + record.data.CHK_UID_NAME
                //},
            },
            {
                text: "已盤",
                dataIndex: 'DONE_NUM',
                width: 80,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "未盤",
                dataIndex: 'UNDONE_NUM',
                width: 80,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "盤點狀態",
                dataIndex: 'DONE_STATUS',
                width: 70
            },
            {
                header: "",
                flex: 1
            }
        ],
    });
    var statusWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        items: [T3Grid],
        modal: true,
        //items: [T21Grid, T22Grid],
        width: "450px",
        //height: 400,
        resizable: false,
        draggable: false,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "盤點狀態",
        buttons: [{
            text: '關閉',
            handler: function () {
                statusWindow.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                statusWindow.setY(0);
            }
        }
    });
    statusWindow.hide();
    //#endregion

    popWinForm = function (param) {
        var src = '../Index/CE0004_INI?no=' + param;
        //var tp = window.parent.tabs;
        var tp = Tabs;
        //如果tab已存在，則跳到該tab
        var target = tp.child('[title=' + param + '盤點明細' + ']');
        if (target != null) {
            //var activeTab = tp.getActiveTab();
            var activeTabIndex = tp.items.findIndex('id', target.id);
            tp.remove(activeTabIndex);
            var newTab = tp.insert(activeTabIndex, Ext.create('TAB.C.CE0004_1', {
                itemId: "tab" + param,
                title: param + '盤點明細',
                closable: true,
                chk_no: param
            }));
            tp.setActiveTab(newTab);
            //tp.setActiveTab(target);
            tabFlag = true;
        }
        else {
            //var iframe1 = document.createElement("IFRAME");
            //iframe1.id = "frame" + param;
            //iframe1.frameBorder = 0;
            //iframe1.src = src;
            //iframe1.height = "100%";
            //iframe1.width = "100%";
            var tabItem = {
                title: param + '盤點明細',
                id: 'tab' + param,
                itemId: "tab" + param,
                html: '<iframe src="' + src + '" id="frame' + param + '" width="100%" height="100%" frameborder="0"></iframe>',
                closable: true,
                //items: [T2Panel]
            };
            //var newTab = tp.add(tabItem);
            //tp.setActiveTab(newTab);

            newTab = tp.add(Ext.create('TAB.C.CE0007_INI', {
                itemId: "tab" + param,
                title: param + '盤點明細',
                closable: true,
                chk_no: param
            }));
            tp.setActiveTab(newTab);
        }
    }

    var Tabs = Ext.widget('tabpanel', {
        itemId: 'rootTabs',
        plain: true,
        border: false,
        resizeTabs: true,
        autoDestroy: false, // 很重要,預設是true,關閉分頁後,宣告的物件也會被destory
        layout: 'fit',
        defaults: {
            layout: 'fit'
        },
        items: [
            {
                itemId: 'Query',
                title: '查詢清單',
                items: [T1Grid]
            },
            //{
            //    itemId: 'Form',
            //    title: '瀏覽',
            //    items: [T1Form, T2Form]
            //}
        ],
        listeners: {
            tabchange: function (me, newCard, oldCard) {
                //if (newCard.itemId.indexOf('tab') == 0 && oldCard.itemId.indexOf('tab') == 0 && tabFlag) {
                if (newCard.itemId.indexOf('tab') == 0 && tabFlag) {
                    //var activeTab = me.getActiveTab();
                    tabFlag = false;
                    var activeTabIndex = me.items.findIndex('id', newCard.id);
                    me.remove(activeTabIndex);
                    var newTab = me.insert(activeTabIndex, Ext.create('TAB.C.CE0007_INI', {
                        itemId: "tab" + newCard.itemId.substring(3),
                        title: newCard.itemId.substring(3) + '盤點明細',
                        closable: true,
                        chk_no: newCard.itemId.substring(3)
                    }));
                    me.setActiveTab(newTab);
                    tabFlag = true;
                }
                else if (newCard.itemId.indexOf('tab') != 0) {
                    //alert('a');
                    T1Load();
                }
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
                itemId: 't1Grid',
                region: 'north',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                //height: 60,
                items: [T1Query]
            },
            {
                itemId: 't2Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                //height: '90%',
                items: [Tabs]
            },
            //{
            //    itemId: 'form',
            //    region: 'east',
            //    collapsible: true,
            //    floatable: true,
            //    width: 300,
            //    title: '瀏覽',
            //    border: false,
            //    collapsed: true,
            //    layout: {
            //        type: 'fit',
            //        padding: 5,
            //        align: 'stretch'
            //    },
            //    items: [T1Form]
            //}
        ]
    });

    var today = new Date();
    var d = today.getMonth() + 1;
    if (d < 10)
        d = '0' + d;
    var year = today.getFullYear() - 1911;
    var dt = year.toString() + d.toString();
    T1Query.getForm().findField('CHK_YM').setValue(dt);
});