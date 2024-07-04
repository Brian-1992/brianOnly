Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

var chk = null;
function setCheck(sender) {
    this.chk = sender;
    //alert(this.chk.id);
};

Ext.onReady(function () {
    var T1Get = '/api/UR1024/GetUsers';
    var T2Get = '/api/UR1024/GetMenu';
    var T2Set = '/api/UR1024/Update';
    var InidComboGet = '/api/BC0002/GetInidCombo';
    var T1Name = '';
    var IsEditMode = false;

    var T1Rec = 0;
    var T1LastRec = null;
    var T1RLNO = '';
    var IsPageLoad = true;

    if (!-[1,]) {
        alert("IE8以下瀏覽器無法使用此功能頁面!建議使用IE9以上或Chrome、Firefox瀏覽器");
        history.go(-1);
    }

    //Roles
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['TUSER', 'UNA', 'TACL_CREATE_BY', 'TACL_CREATE_UNA', 'ADUSER']
    });

    var T1InidCombo = Ext.create('WEBAPP.form.UrInidCombo', {
        name: 'INID',
        fieldLabel: '責任中心',
        labelWidth: 70,
        limit: 20,
        queryUrl: InidComboGet
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: "right",
            labelWidth: 60
        },

        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            defaultType: 'textfield',
            items: [{
                fieldLabel: '帳號',
                name: 'P0',
                enforceMaxLength: true,
                maxLength: 20,
                width: 150,
                padding: '0 4 0 4'
            }, {
                fieldLabel: '姓名',
                name: 'P1',
                enforceMaxLength: true,
                maxLength: 30,
                width: 150,
                padding: '0 4 0 4'
            }, T1InidCombo]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            defaultType: 'textfield',
            items: [{
                fieldLabel: 'AD帳號',
                name: 'P2',
                enforceMaxLength: true,
                maxLength: 30,
                width: 150,
                padding: '0 4 0 4'
            }, {
                xtype: 'button',
                text: '查詢',
                handler: function () { T1Tool.moveFirst(); }
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P0').focus();
                }
            }
            ]
        }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'TUSER', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: '', //TACL_CREATE_BY
                    p1: T1Query.getForm().findField('P0').getValue(),
                    p2: T1Query.getForm().findField('P1').getValue(),
                    p3: T1Query.getForm().findField('INID').getValue(),
                    p4: '',
                    p5: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                T2Store.getRootNode().removeAll();
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'POST',
                destroy: 'POST'
            },
            url: T1Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T1Load() {
        T1Store.loadPage(1);
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            dock: 'top',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T1Tool]
        }
        ],

        // grid columns
        columns: [
        { xtype: 'rownumberer' },
        {
            text: "帳號",
            dataIndex: 'TUSER',
            width: 100
        }, {
            text: "姓名",
            dataIndex: 'UNA',
            width: 100
        }, {
            text: "AD帳號",
            dataIndex: 'ADUSER',
            width: 100
        }, {
            text: "新增人員帳號",
            dataIndex: 'TACL_CREATE_BY',
            width: 100
        }, {
            text: "新增人員姓名",
            dataIndex: 'TACL_CREATE_UNA',
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                myMask.show();
                var toolbar = T2Tab.activeTab.toolbar;
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        resetButtonStatus();
        T1TUSER = '';
        if (T1LastRec) {
            //isNew = false;
            T1TUSER = T1LastRec.get('TUSER');
        }
        if (T1TUSER.length > 0) {
            T2Store.load({
                params: {
                    p0: T1TUSER,
                    PG: null
                },
                callback: function () {
                    myMask.hide();
                }
            });
        }
        else
        {
            myMask.hide();
        }
    }

    function resetButtonStatus()
    {
        var activeTree = T2Tab.activeTab.tree;
        activeTree.down('#edit').setDisabled(T1Rec == 0);
        activeTree.down('#submit').setDisabled(true);
        activeTree.down('#cancel').setDisabled(true);
    }
    
    //權限樹的資料模型
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'FG', type: 'string' },
            { name: 'PG', type: 'string' },
            //{ name: 'SG', type: 'string' },
            { name: 'FT', type: 'string' },
            { name: 'FS', type: 'string' },
            { name: 'FD', type: 'string' },
            //{ name: 'F0', type: 'string' },
            { name: 'text', type: 'string' },
            //{ name: 'url', type: 'string' },
            { name: 'A', type: 'boolean' },
            //{ name: 'G', type: 'boolean' },
            //{ name: 'S', type: 'boolean' },
            { name: 'V', type: 'boolean' },
            { name: 'R', type: 'boolean' },
            { name: 'U', type: 'boolean' },
            { name: 'P', type: 'boolean' },
            { name: 'HV', type: 'boolean' },
            { name: 'HR', type: 'boolean' },
            { name: 'HU', type: 'boolean' },
            { name: 'HP', type: 'boolean' }
        ]
    });

    function getTreeStore() {
        return Ext.create('Ext.data.TreeStore', {
            model: 'T2Model',
            defaultRootProperty: 'etts',
            nodeParam: 'PG',
            proxy: {
                type: 'ajax',
                actionMethods: {
                    create: 'POST',
                    read: 'POST',
                    update: 'POST',
                    destroy: 'POST'
                },
                url: T2Get,
                listeners: {
                    exception: function (proxy, response, operation) {
                        var e = Ext.JSON.decode(response.responseText);
                        Ext.MessageBox.show({
                            title: 'REMOTE EXCEPTION',
                            msg: e.ExceptionMessage,
                            icon: Ext.MessageBox.ERROR,
                            buttons: Ext.Msg.OK
                        });
                    }
                }
            },
            listeners: {
                beforeload: function (store, operation, options) {
                    if (IsPageLoad) { return false; }
                    else {
                        if (T1LastRec) {
                            operation.config.params.p0 = T1LastRec.get('TUSER');
                            operation.config.params.p1 = T1LastRec.get('TACL_CREATE_BY');
                        }
                    }
                },
                load: function (store, records, success, eOpts) {
                    myMask.hide();
                    if (success) { store.tree.expandAll(); }
                    //if (success) { store.tree.expandNode(store.getRoot()); }
                },
                nodeappend: function (thisNode, newNode, newNodeIndex, eOpt) {
                    //if (this.tree)
                    //    this.tree.expandNode(newNode);
                }
            }
        });
    }
    var T2Store = getTreeStore();
    //var T2Store2 = getTreeStore('MOBILE');
    
    function getToolbar() {
        return Ext.create('Ext.toolbar.Toolbar', {
            items: [{
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    var activeTree = T2Tab.activeTab.tree;
                    var activeToolbar = T2Tab.activeTab.toolbar;
                    activeTree.columns[1].setDisabled(false);
                    activeTree.columns[2].setDisabled(false);
                    activeTree.columns[3].setDisabled(false);
                    activeTree.columns[4].setDisabled(false);
                    activeTree.columns[5].setDisabled(false);
                    activeToolbar.down('#edit').setDisabled(true);
                    activeToolbar.down('#submit').setDisabled(false);
                    activeToolbar.down('#cancel').setDisabled(false);
                    IsEditMode = true;
                    T1Grid.mask();
                }
            }, {
                itemId: 'submit', text: '儲存', disabled: true, handler: function () {
                    Ext.MessageBox.confirm('權限修改', '是否確定修改權限?', function (btn, text) {
                        if (btn === 'yes') {
                            T2Submit();
                        }
                    }
                    )
                }
            }, {
                    itemId: 'cancel', text: '取消', disabled: true, handler: function () {
                        var activeTree = T2Tab.activeTab.tree;
                        var activeToolbar = T2Tab.activeTab.toolbar;
                        activeTree.columns[1].setDisabled(true);
                        activeTree.columns[2].setDisabled(true);
                        activeTree.columns[3].setDisabled(true);
                        activeTree.columns[4].setDisabled(true);
                        activeTree.columns[5].setDisabled(true);
                        activeToolbar.down('#edit').setDisabled(false);
                        activeToolbar.down('#submit').setDisabled(true);
                        activeToolbar.down('#cancel').setDisabled(true);
                        setFormT1a();
                        IsEditMode = false;
                        T1Grid.unmask();
                }
            },
            {
                labelAlign: 'right',
                labelWidth: 100,
                width: 250,
                xtype: 'triggerfield',
                fieldLabel: '編號或名稱篩選',
                triggerCls: 'x-form-clear-trigger',
                onTriggerClick: function () {
                    var store = this.up('treepanel').store;

                    this.reset();
                    store.clearFilter();
                    this.focus();
                },
                listeners: {
                    change: function () {
                        var tree = this.up('treepanel'),
                            v,
                            matches = 0;
                        try {
                            v = new RegExp(this.getValue(), 'i');
                            tree.store.clearFilter();
                            tree.store.filter({
                                filterFn: function (node) {
                                    if (!node) return;

                                    var children = node.childNodes,
                                        len = children && children.length,
                                        visible = node.isLeaf() ? v.test(node.get('FG')) || v.test(node.get('F1')) : true,
                                        i;

                                    //for (i = 0; i < len && !(visible = children[i].get('visible')); i++);
                                    //for (i = 0; i < len && !(visible = v.test(children[i].get('FG'))); i++);

                                    //alert(node.get('FG') + '的child有:' + len + '個');

                                    return visible;
                                }
                            });
                            //tree.down('#matches').setValue(matches);
                        } catch (e) {
                            
                            this.markInvalid('Invalid regular expression');
                        }
                    },
                    buffer: 250
                }
            }],
            frame: false,
            border: false,
            plain: true
        });
    }
    var T2Tool = getToolbar();
    var T2Tool2 = getToolbar();

    var T2Desc = Ext.create('Ext.toolbar.Toolbar', {
        items: [{
            xtype: 'displayfield',
            width: 600,
            name: 'memo',
            renderer: function (val, meta, record) {
                return '<span>查詢 : 顯示於左方功能樹，並可執行查詢功能<br />維護 : 可維護資料，包括新增、修改、刪除<br />列印 : 可列印或匯出資料<span>';
            }
        }],
        frame: false,
        border: false,
        plain: true
    });

    function selectAll(checked, rowIndex) {
        var authType = ['V', 'U', 'P', 'R'];
        var record = T2Store.getAt(rowIndex);
        Ext.each(authType, function (authT, indexT, valueT) {
            trySetCheck(authT, checked, record);
        });
    }

    function trySetCheck(dataIndex, checked, record) {
        if (record.get('H' + dataIndex)) {
            if (record.get(dataIndex) == !checked)
                record.set(dataIndex, checked);
            tryCheckAll(dataIndex, checked, record)
        }
        else {
            record.set(dataIndex, false);
        }
    }

    var tryCheckAll = function (dataIndex, checked, record) {
        if (!record.data.leaf) {
            record.cascadeBy(function (n) {
                if (n.get('H' + dataIndex))
                    n.set(dataIndex, checked);
                else
                    n.set(dataIndex, false);
            });
        }

        if (checked) {
            record.bubble(function (n) {
                if (n.get('FG'))
                    n.set(dataIndex, checked);
            });
        }
    };

    var checkAll = function (column, recordIndex, checked, record) {
        tryCheckAll(column.dataIndex, checked, record);
    };

    var setCheckbox = function (meta, hasAuth) {
        if (!IsEditMode)
            meta['tdCls'] = 'x-item-disabled';
        if (!hasAuth) {
            meta['style'] = 'visibility: hidden;';
            meta['tdStyle'] = 'visibility: hidden;';
        }
    }
    
    function getTreePanel(toolbar, store) {
        return Ext.create('Ext.tree.Panel', {
            title: '',
            header: false,
            collapsible: false,
            useArrows: false,
            rootVisible: false,
            store: store,
            multiSelect: false,
            singleExpand: false,
            cls: 'T1',

            dockedItems: [{
                dock: 'top',
                items: [toolbar]
            }],

            columns: [
                {
                    text: '編號',
                    dataIndex: 'FG',
                    width: 60
                }, {
                    text: '全選',
                    width: 60,
                    align: 'left',
                    xtype: 'checkcolumn',
                    dataIndex: 'A',
                    disabled: true,
                    menuDisabled: true,
                    stopSelection: false,
                    listeners: {
                        'checkchange': function (cb, rowIndex, checked, eOpts) {
                            selectAll(checked, rowIndex);
                        }
                    }
                }, {
                    text: '顯示',
                    width: 60,
                    align: 'left',
                    xtype: 'checkcolumn',
                    dataIndex: 'V',
                    disabled: true,
                    hidden: true,
                    menuDisabled: true,
                    stopSelection: false,
                    listeners: {
                        checkchange: checkAll
                    }
                }, {
                    text: '查詢',
                    width: 60,
                    align: 'left',
                    xtype: 'checkcolumn',
                    dataIndex: 'R',
                    disabled: true,
                    menuDisabled: true,
                    stopSelection: false,
                    renderer: function (value, meta, record, rowIndex, colIndex, store) {
                        setCheckbox(meta, record.data.HR);
                        return new Ext.ux.CheckColumn().renderer(value);
                    },
                    listeners: {
                        checkchange: checkAll
                    }
                }, {
                    text: '維護',
                    width: 60,
                    align: 'left',
                    xtype: 'checkcolumn',
                    dataIndex: 'U',
                    disabled: true,
                    menuDisabled: true,
                    stopSelection: false,
                    renderer: function (value, meta, record, rowIndex, colIndex, store) {
                        setCheckbox(meta, record.data.HU);
                        return new Ext.ux.CheckColumn().renderer(value);
                    },
                    listeners: {
                        checkchange: checkAll
                    }
                }, {
                    text: '列印',
                    width: 60,
                    align: 'left',
                    xtype: 'checkcolumn',
                    tdCls: 'checkBoxColumn',
                    dataIndex: 'P',
                    disabled: true,
                    menuDisabled: true,
                    stopSelection: false,
                    renderer: function (value, meta, record, rowIndex, colIndex, store) {
                        setCheckbox(meta, record.data.HP);
                        return new Ext.ux.CheckColumn().renderer(value);
                    },
                    listeners: {
                        checkchange: checkAll
                    }
                }, {
                    xtype: 'treecolumn',
                    dataIndex: 'text',
                    flex: 1
                }],

            listeners: {
                //beforecollapse: function (sender, eOpts) {
                //beforehide: function (sender, eOpts) {
                //    alert("Hide not supported!");
                //    return false;
                //},
                itemclick: function (view, node, item, index, e) {

                    if (e.getTarget() && e.getTarget().type == "checkbox") {
                        setCheck(e.getTarget());
                        var t = chk.id.substring(chk.id.length - 1);
                        var c = chk.checked;

                        if (c) {

                            node.bubble(function (n) {
                                var fg = n.get("FG");
                                if (fg.length > 0) {
                                    var eg = document.getElementById(fg + t);
                                    eg.checked = c;
                                }
                            });
                        }
                        node.cascadeBy(function (n) {
                            var fg = n.get("FG");
                            if (fg.length > 0) {
                                var eg = document.getElementById(fg + t);
                                eg.checked = c;
                            }
                        });
                    }
                }
            }
        });
    }
    var T2Tree = getTreePanel(T2Tool, T2Store); T2Store.tree = T2Tree;
    //var T2Tree2 = getTreePanel(T2Tool2, T2Store2); T2Store2.tree = T2Tree2;

    var T2Form = Ext.widget({
        xtype: 'form',
        defaultType: 'hidden',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            value: 'u',
            xtype: 'hidden'
        }, {
            //    fieldLabel: '帳號',
            //    name: 'TUSER',
            //    allowBlank: false,
            //    allowOnlyWhitespace: false,
            //    enforceMaxLength: true,
            //    maxLength: 20,
            //    fieldCls: 'required',
            //    readOnly: true,
            //    value: 'admin'
            //}, {
            name: 'TUSER',
        }, {
            name: 'SG'
        }, {
            name: 'JSON'
        }]
    });
    function T2Submit() {
        resetButtonStatus();
        var f = T2Form.getForm();
        myMask.show();
        var t1 = [];
        var activeTree = T2Tab.activeTab.tree;
        var activeStore = activeTree.getStore();
        var activeToolbar = T2Tab.activeTab.toolbar;

        //var data = activeStore.getData();
        var data = activeStore.getModifiedRecords();
        var len = data.length;

        for (var i = 0; i < len; i++)
        {
            //var rec = data.getAt(i);
            var rec = data[i];
            //alert(data.getAt(i).get('FG'));
            var fg = rec.get('FG');
            //var g = r.get('G') ? 1 : 0;
            //var s = r.get('S') ? 1 : 0;
            var v = rec.get('V') ? 1 : 0;
            var r = rec.get('R') ? 1 : 0;
            var u = rec.get('U') ? 1 : 0;
            var p = rec.get('P') ? 1 : 0;
            //t1.push({ "RLNO": T1RLNO, "FG": fg, "V": v, "R": r, "U": u, "P": p });
            //V的值跟R一樣
            t1.push({ "TUSER": T1TUSER, "FG": fg, "V": r, "R": r, "U": u, "P": p });
        }

        var ds = {
            T1: t1
        };
        var json = JSON.stringify(ds);
        f.findField('JSON').setValue(json); //T2Store.proxy.reader.rawData
        f.findField('TUSER').setValue(T1TUSER);
        f.submit({
            url: T2Set,
            success: function (form, action) {
                //var data = activeStore.getData();
                //var len = data.length;
                for (var i = 0; i < len; i++) {
                    data[i].commit();
                    //var r = data.getAt(i);
                    //r.commit();
                }
                activeTree.columns[1].setDisabled(true);
                activeTree.columns[2].setDisabled(true);
                activeTree.columns[3].setDisabled(true);
                activeTree.columns[4].setDisabled(true);
                activeTree.columns[5].setDisabled(true);
                activeToolbar.down('#edit').setDisabled(false);
                activeToolbar.down('#submit').setDisabled(true);
                activeToolbar.down('#cancel').setDisabled(true);
                IsEditMode = false;
                msglabel('資料修改成功');
                myMask.hide();
                T1Grid.unmask();
            },
            failure: function (form, action) {
                IsEditMode = false;
                myMask.hide();
                T1Grid.unmask();
                switch (action.failureType) {
                    case Ext.form.action.Action.CLIENT_INVALID:
                        Ext.Msg.alert('錯誤', MMIS.Message.clientError);
                        break;
                    case Ext.form.action.Action.CONNECT_FAILURE:
                        Ext.Msg.alert('錯誤', MMIS.Message.communicationError);
                        break;
                    case Ext.form.action.Action.SERVER_INVALID:
                        Ext.Msg.alert('錯誤', action.result.msg);
                        break;
                }
            }
        });
    }

    var T2Tab = Ext.widget('tabpanel', {
        plain: true,
        resizeTabs: true,
        deferredRender: false,
        cls: 'tabpanel',
        defaults: {
            layout: 'fit',
        },
        items: [{
            id: 'tabClassic',
            //title: '電腦版程式',
            title: '所有程式',
            items: [T2Tree],
            tree: T2Tree,
            toolbar: T2Tool,
            closable: false
        }, {
            id: 'tabMobile',
            title: '行動版程式',
            //items: [T2Tree2],
            //tree: T2Tree2,
            //toolbar: T2Tool2,
            closable: false,
            hidden: true
        }],
        listeners: {
            //頁籤字體改變顏色 by 吉威 2018/10/19
            tabchange: function (tabPanel, newCard, oldCard, eOpts) {
                //oldCard.tree.getSelectionModel().deselectAll();
                //T1Rec = 0;
                //T1LastRec = null;
                resetButtonStatus();
                if (oldCard) {
                    oldCard.tab.btnInnerEl.setStyle('color', 'black');
                }
                if (newCard) {
                    newCard.tab.btnInnerEl.setStyle('color', 'brown');
                }
            }
        }
    });

    var T2Panel = Ext.create('Ext.panel.Panel', {
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            region: 'north',
            items: [T2Desc]
        }, {
            region: 'center',
            layout: 'fit',
            items: [T2Tab]
        }, {
            region: 'south',
            xtype: 'form',
            hidden: true,
            items: [T2Form]
        }],
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
            split: true,
            width: '45%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T1Grid]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: false,
            split: true,
            width: '55%',
            minWidth: 120,
            minHeight: 140,
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T2Panel]
        }
        ]
    });
    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });

    T1Query.getForm().findField('P0').focus();
    IsPageLoad = false;
    //T1Load();
});